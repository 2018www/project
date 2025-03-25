using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BlogA.Models;
using BlogA.DAL;
using BlogA.ModelViews;
using MongoDB.Driver.Linq;
using System.Net;

namespace BlogA.Services
{
    public class BookService
    {
        private IMongoCollection<Book> _bookCollection;
        private IMongoCollection<Chapter> _chapterCollection;
        private IMongoCollection<Book> _bookCollectionReader;
        private readonly Sequence _seqService;
        private readonly ChapterService _chapterService;
        private readonly CommentService _commentService;

        public BookService(DatabaseConnection dbConnection, ReaderDBConnection readerDBConnection,Sequence seqService, ChapterService chapterService, CommentService commentService)
        {
            _bookCollection = dbConnection.Book;
            _chapterCollection = dbConnection.Chapter;
            _bookCollectionReader = readerDBConnection.Book;
            _seqService = seqService;
            _chapterService = chapterService;
            _commentService = commentService;

        }

        public async Task<List<BookView>> GetAllBook()
        {
            try
            {
                List<BookView> bookList = await _bookCollection.AsQueryable()
                .Select(b => new BookView
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Introduction = b.Introduction,
                    Era = b.Era,
                    Sexuality = b.Sexuality,
                    TagList = b.TagList,
                    CharacterPool = b.CharacterPool != null ? b.CharacterPool.Select(c => new CharacterInfoView
                    {
                        IdInList = c.IdInList,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        LastName_Front = c.LastName_Front,
                        Description = c.Description,
                    }).ToList() : null,
                    SectionList = b.SectionList.Select(s => new SectionView
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DisplaySequence = s.DisplaySequence,
                        Era = s.Era,
                        Description = s.Description,
                        MainCharacterId = s.MainCharacterId,
                        Public = s.IsPublic,

                    }).ToList(),
                    Public = b.IsPublic,
                    Finished = b.Finished,
                    Original = b.IsOriginal,
                    Meat = b.HasMeat,
                    AllowComment = b.AllowComment,
                    MaxCommentLength = b.MaxCommentLength,
                    NewestChapterId = b.NewestChapterId == null ? null : b.NewestChapterId
                }).ToListAsync();

                //get newest chapter heading for each book
                if (bookList.Count > 0)
                {
                    List<int> bookSeq = await _seqService.GetBookSequence();
                    foreach (var book in bookList)
                    {
                        int seq = bookSeq.IndexOf(book.BookId);
                        if(seq == -1)
                        {
                            throw new Exception($"error on getting all books. Book({book.BookId}) is not in the book_seq");
                        }
                        book.DisplaySequence = seq;

                        if (book.NewestChapterId != null)
                        {
                            var heading = await _chapterService.GetHeadingByChapterId(book.NewestChapterId);

                            if (heading != null)
                            {
                                book.NewestChapterHeading = heading;
                            }
                            else
                            {
                                throw new Exception($"Error on BookService - get all book, book({book.BookId}), newest chapter({book.NewestChapterId}), while set the newest chapter, missing chapter heading or this chapter NOT in chapter collection");
                            }

                            int secId = await _chapterService.GetNewestChapterBelongSectionId(book.NewestChapterId);
                            if (secId > 0)
                            {
                                book.NewestChapterBelongSecId = secId;
                            }
                            else
                            {
                                throw new Exception($"Error on BookService - get all book, book({book.BookId}), newest chapter({book.NewestChapterId}), while set the newest chapter, cannot find the belonged section Id");
                            }
                        }

                        //get chapter count for each section of the book
                        foreach (var sec in book.SectionList) 
                        { 
                            var filter = Builders<Chapter>.Filter.Eq(c=>c.BookId, book.BookId) & Builders<Chapter>.Filter.Eq(c => c.SectionId, sec.Id);

                            long count = await _chapterCollection.CountDocumentsAsync(filter);
                            int chCount = Convert.ToInt32(count);
                            sec.ChapterCount = chCount;
                        }

                    }

                    bookList = bookList.OrderBy(b => b.DisplaySequence).ToList();

                }

                return bookList;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting all books, in book service: {ex}");
            }

        }

        public async Task<BookView?> GetBookById(int id)
        {
            try
            {
                BookView book = await _bookCollection.AsQueryable()
                    .Where(x => x.BookId == id)
                .Select(b => new BookView
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Introduction = b.Introduction,
                    Era = b.Era,
                    Sexuality = b.Sexuality,
                    TagList = b.TagList,
                    CharacterPool = b.CharacterPool != null ? b.CharacterPool.Select(c => new CharacterInfoView
                    {
                        IdInList = c.IdInList,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        LastName_Front = c.LastName_Front,
                        Description = c.Description,
                    }).ToList() : null,
                    SectionList = b.SectionList.Select(s => new SectionView
                    {
                        Id = s.Id,
                        Name = s.Name,
                        DisplaySequence = s.DisplaySequence,
                        Era = s.Era,
                        Description = s.Description,
                        MainCharacterId = s.MainCharacterId,
                        Public = s.IsPublic,

                    }).ToList(),
                    Public = b.IsPublic,
                    Finished = b.Finished,
                    Original = b.IsOriginal,
                    Meat = b.HasMeat,
                    AllowComment = b.AllowComment,
                    MaxCommentLength = b.MaxCommentLength,
                    NewestChapterId = b.NewestChapterId == null ? null : b.NewestChapterId
                }).FirstOrDefaultAsync();

                //get newest chapter heading
                if (book != null)
                {
                    foreach (var sec in book.SectionList)
                    {
                        var filter = Builders<Chapter>.Filter.Eq(c => c.BookId, book.BookId) & Builders<Chapter>.Filter.Eq(c => c.SectionId, sec.Id);

                        long count = await _chapterCollection.CountDocumentsAsync(filter);
                        int chCount = Convert.ToInt32(count);
                        sec.ChapterCount = chCount;
                    }

                    if(book.NewestChapterId != null)
                    {
                        var heading = await _chapterService.GetHeadingByChapterId(book.NewestChapterId);

                        if (heading != null)
                        {
                            book.NewestChapterHeading = heading;
                        }
                        else
                        {
                            throw new Exception($"Error on BookService - get one book, book({book.BookId}), newest chapter({book.NewestChapterId}), while set the newest chapter, missing chapter heading or this chapter NOT in chapter collection");
                        }
                        int secId = await _chapterService.GetNewestChapterBelongSectionId(book.NewestChapterId);
                        if (secId > 0)
                        {
                            book.NewestChapterBelongSecId = secId;
                        }
                        else
                        {
                            throw new Exception($"Error on BookService - get all book, book({book.BookId}), newest chapter({book.NewestChapterId}), while set the newest chapter, cannot find the belonged section Id");
                        }
                    }
                }
                return book;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting the book ({id}), in book service: {ex}");
            }
        }
        public async Task<List<int>> GetBookSeqence()
        {
            return await _seqService.GetBookSequence();
        }

        public async Task UpdateBookSeqence(List<int> newSeq)
        {
            await _seqService.UpdateBookSequence(newSeq);
        }

        public async Task<int> CreateBook(BookView bookView)
        {
            List<int> existedSeq = await _seqService.GetBookSequence();
            try
            {
                Book book = ConvertDTOToBook(bookView);

                int nextId = await _seqService.GetBookId();
                book.BookId = nextId;
                bookView.BookId = nextId;

                //add the new book to the book sequence list, will be last index automatically
                List<int> newSeq = existedSeq.Select(x => x).ToList();
                if (newSeq.Contains(book.BookId))
                {
                    await _seqService.CancelBookId();
                    throw new Exception($"error on CreateBook, update seq, this id {nextId} already in book_seq");
                }
                else
                {
                    newSeq.Add(book.BookId);
                    await _seqService.UpdateBookSequence(newSeq);
                    await _bookCollection.InsertOneAsync(book);
                    await _bookCollectionReader.InsertOneAsync(book);

                    return book.BookId;
                }
            }
            catch (Exception ex)
            {
                //roll back if there are errors
                await _bookCollection.DeleteOneAsync(b => b.BookId == bookView.BookId);
                await _bookCollectionReader.DeleteOneAsync(b => b.BookId == bookView.BookId);
                await _seqService.CancelBookId();
                await _seqService.UpdateBookSequence(existedSeq);
                throw new Exception($"error on create book, in book service: {ex}");

            }

        }

        public async Task UpdateBookWhole(int id, BookView bookView)
        {
            try
            {
                //use set to update, so that no expose the objectID 
                var filter = Builders<Book>.Filter.Eq(b => b.BookId, id);
                UpdateDefinition<Book>? generalUpdate = null;
                UpdateDefinition<Book>? characterPoolUpdate = null;

                //get the sectionIds to change the chapter, comment collections later, and the book_chapters_seq      
                List<int> sectionIdsToBeDeleted = new();
                var existedSections = await _bookCollection.AsQueryable()
                    .Where(b => b.BookId == id)
                    .Select(x => x.SectionList).FirstOrDefaultAsync();
                List<int> originalSectionIds = existedSections.Select(x => x.Id).ToList();

                List<Section> newSectionList = new();
                if (bookView.SectionList != null)
                {
                    //chekc if there is duplicated DisplaySequence in sectionList
                    var duplicated = bookView.SectionList.GroupBy(x => x.DisplaySequence).Where(c => c.Count() > 1).Select(a => a.Key).Any();
                    if (duplicated)
                    {
                        throw new Exception("Update failed. Duplicated DisplaySequence in book section list.");
                    }

                    foreach (var item in bookView.SectionList)
                    {
                        var section = ConvertDTOToSection(item);
                        newSectionList.Add(section);

                    }

                    List<int> newSecIds = newSectionList.Select(x => x.Id).ToList();
                    foreach(var secId in originalSectionIds)
                    {
                        if (!newSecIds.Contains(secId))
                        {
                            sectionIdsToBeDeleted.Add(secId);
                        }
                    }
                }

                generalUpdate = Builders<Book>.Update
                       .Set(b => b.Title, bookView.Title)
                       .Set(b => b.Introduction, bookView.Introduction)
                       .Set(b => b.Era, bookView.Era)
                       .Set(b => b.Sexuality, bookView.Sexuality)
                       .Set(b => b.TagList, bookView.TagList)
                       .Set(b => b.SectionList, newSectionList)
                       .Set(b => b.Finished, bookView.Finished)
                       .Set(b => b.IsPublic, bookView.Public)
                       .Set(b => b.IsOriginal, bookView.Original)
                       .Set(b => b.HasMeat, bookView.Meat)
                       .Set(b => b.AllowComment, bookView.AllowComment)
                       .Set(b => b.MaxCommentLength, bookView.MaxCommentLength);

                List<CharacterInfo> charInfoList = new();
                if (bookView.CharacterPool != null && bookView.CharacterPool.Count > 0)
                {
                    foreach (var item in bookView.CharacterPool)
                    {
                        var charInfo = ConvertDTOToCharacterInfo(item);
                        charInfoList.Add(charInfo);
                    }
                    characterPoolUpdate = Builders<Book>.Update.Set(b => b.CharacterPool, charInfoList);
                }
                else if (bookView.CharacterPool == null || bookView.CharacterPool.Count == 0)
                {
                    characterPoolUpdate = Builders<Book>.Update.Unset(b => b.CharacterPool);
                }

                var totalUpdate = Builders<Book>.Update.Combine(generalUpdate, characterPoolUpdate);
                var result = await _bookCollection.UpdateOneAsync(filter, totalUpdate);
                await _bookCollectionReader.UpdateOneAsync(filter, totalUpdate);

                //if updated successfully, modify chapter and comment and bookChapterSeq accordingly
                if (result.IsAcknowledged)
                {
                    if (sectionIdsToBeDeleted.Count > 0)
                    {
                        //delete chapter by bookId,SectionId
                        await _chapterService.DeleteChapterByBookSectionId(bookView.BookId, sectionIdsToBeDeleted);

                        //delete any sections from book_chapters_seq
                        Dictionary<int, List<int>> existedSecChSeq = await _seqService.GetOneBookChapterSequence(bookView.BookId);
                        if (existedSecChSeq.Count > 0)
                        {
                            foreach (var secId in sectionIdsToBeDeleted)
                            {
                                //the author may create 10 sections but non of them have chapters, then the book_chapter_seq will not have any records of these sections
                                if (existedSecChSeq.ContainsKey(secId))
                                {
                                    existedSecChSeq.Remove(secId);
                                }
                            }
                            await _seqService.UpdateOneBookChapterSequence(id, existedSecChSeq);
                        }

                        //delete any related comments from commentCollection
                        await _commentService.DeleteRelatedCommentsByBookSecChIds(id,sectionIdsToBeDeleted, null);
                    }
                }
                else
                {
                    throw new Exception($"error on update book({id}), in book service, database not process the request");
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"error on update book({id}), in book service: {ex}");
            }
        }

        public async Task UpdateBookBasic(int id, BookView bookView)
        {
            try
            {
                //Update basic info - everything except SectionList & CharacterList 
                var filter = Builders<Book>.Filter.Eq(b => b.BookId, id);
                UpdateDefinition<Book>? generalUpdate = null;

                //update default section's era if book's era has been changed
                var defaultSec = bookView.SectionList.Where(x => x.Id == 1).FirstOrDefault();
                if (defaultSec != null && defaultSec.Era != bookView.Era)
                {
                    defaultSec.Era = bookView.Era;
                    int index = bookView.SectionList.IndexOf(defaultSec);
                    if (index != -1)
                    {
                        bookView.SectionList[index] = defaultSec;
                    }
                }
                List<Section> sections = new();
                foreach (var item in bookView.SectionList)
                {
                    var section = ConvertDTOToSection(item);
                    sections.Add(section);
                }


                generalUpdate = Builders<Book>.Update
                       .Set(b => b.Title, bookView.Title)
                       .Set(b => b.Introduction, bookView.Introduction)
                       .Set(b => b.Era, bookView.Era)
                       .Set(b => b.Sexuality, bookView.Sexuality)
                       .Set(b => b.TagList, bookView.TagList)
                       .Set(b => b.Finished, bookView.Finished)
                       .Set(b => b.IsPublic, bookView.Public)
                       .Set(b => b.IsOriginal, bookView.Original)
                       .Set(b => b.HasMeat, bookView.Meat)
                       .Set(b => b.AllowComment, bookView.AllowComment)
                       .Set(b => b.MaxCommentLength, bookView.MaxCommentLength)
                                              .Set(b => b.SectionList, sections);


                var result = await _bookCollection.UpdateOneAsync(filter, generalUpdate);
                if (!result.IsAcknowledged)
                {
                    throw new Exception($"error on update book({id}), in book service, database not process the request");
                }
                await _bookCollectionReader.UpdateOneAsync(filter, generalUpdate);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on update basic book({id}), in book service: {ex}");
            }
        }

        public async Task UpdateBookSectionList(int id, BookView bookView)
        {
            try
            {
                if (bookView.SectionList ==null || bookView.SectionList.Count == 0)
                {
                    throw new Exception("Updated failed. A book must contain at least one section.");
                }

                if (bookView.SectionList.Count > 10)
                {
                    throw new Exception("Updated failed. A book cannot have more than 10 sections.");
                }
                var filter = Builders<Book>.Filter.Eq(b => b.BookId, id);

                //get the sectionIds to change the chapter, comment collections later, and the book_chapters_seq      
                List<int> sectionIdsToBeDeleted = new();
                var existedSections = await _bookCollection.AsQueryable()
                    .Where(b => b.BookId == id)
                    .Select(x => x.SectionList).FirstOrDefaultAsync();
                List<int> originalSectionIds = existedSections.Select(x => x.Id).ToList();

                List<Section> newSectionList = new();
                if (bookView.SectionList != null)
                {
                    //chekc if there is duplicated DisplaySequence in sectionList
                    var duplicated = bookView.SectionList.GroupBy(x => x.DisplaySequence).Where(c => c.Count() > 1).Select(a => a.Key).Any();
                    if (duplicated)
                    {
                        throw new Exception("Update failed. Duplicated DisplaySequence in book section list.");
                    }

                    foreach (var item in bookView.SectionList)
                    {
                        var section = ConvertDTOToSection(item);
                        newSectionList.Add(section);

                    }

                    List<int> newSecIds = newSectionList.Select(x => x.Id).ToList();
                    foreach (var secId in originalSectionIds)
                    {
                        if (!newSecIds.Contains(secId))
                        {
                            sectionIdsToBeDeleted.Add(secId);
                        }
                    }

                }

                var update = Builders<Book>.Update.Set(b => b.SectionList, newSectionList);

                var result = await _bookCollection.UpdateOneAsync(filter, update);
                await _bookCollectionReader.UpdateOneAsync(filter, update);

                //if updated successfully, modify chapter and comment and bookChapterSeq accordingly
                if (result.IsAcknowledged)
                {
                    if (sectionIdsToBeDeleted.Count > 0)
                    {
                        //delete chapter by bookId,SectionId
                        await _chapterService.DeleteChapterByBookSectionId(bookView.BookId, sectionIdsToBeDeleted);

                        //delete any sections from book_chapters_seq
                        Dictionary<int, List<int>> existedSecChSeq = await _seqService.GetOneBookChapterSequence(bookView.BookId);
                        if (existedSecChSeq.Count > 0)
                        {
                            foreach (var secId in sectionIdsToBeDeleted)
                            {
                                //the author may create 10 sections but non of them have chapters, then the book_chapter_seq will not have any records of these sections
                                if (existedSecChSeq.ContainsKey(secId))
                                {
                                    existedSecChSeq.Remove(secId);
                                }
                            }
                            await _seqService.UpdateOneBookChapterSequence(id, existedSecChSeq);
                        }

                        //delete any related comments from commentCollection
                        await _commentService.DeleteRelatedCommentsByBookSecChIds(id, sectionIdsToBeDeleted, null);
                    }
                }
                else
                {
                    throw new Exception($"error on update book({id}) section list, in book service, database not process the request");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error on update book({id}) sectionList, in book service: {ex}");
            }
        }

        public async Task UpdateBookCharacterList (int id, BookView bookView)
        {
            try
            {
                var filter = Builders<Book>.Filter.Eq(b => b.BookId, id);

                var characterViews = bookView.CharacterPool;
                List<int> characterViewIds = new();
                if(characterViews != null && characterViews.Count>0)
                {
                    characterViewIds = characterViews.Select(x => x.IdInList).ToList();
                }

                var originalInfo = await _bookCollection.AsQueryable()
                    .Where(b=>b.BookId==id)
                    .Select(x=> new 
                    {
                        BookId = x.BookId,
                        SectionList = x.SectionList,
                        CharacterPool = x.CharacterPool
                    }).FirstOrDefaultAsync();

                var originalCharacters = originalInfo.CharacterPool;
                List<int> originalCharacterIds = new();
                if(originalCharacters != null && originalCharacters.Count>0)
                {
                    originalCharacterIds = originalCharacters.Select(x => x.IdInList).ToList();
                }

                //if both update and existed character are empty, no need to update
                if(characterViewIds.Count==0&&originalCharacterIds.Count==0)
                {
                    return;
                }

                //if delete characters from the pool, check if the character is some section's main role, and if the character is marked in some chapters
                List<int> characterToBeRemoved = new();
                characterToBeRemoved = originalCharacterIds.Where(i=> !characterViewIds.Contains(i)).Select(x=>x).ToList();

                if (characterToBeRemoved.Count > 0)
                {
                    //check if it is main character for any sections
                    var originalSections = originalInfo.SectionList;
                    var mainCharacterIdlist = originalSections.Where(s => s.MainCharacterId != null && s.MainCharacterId.Count>0).Select(x => x.MainCharacterId).ToList();
                    if (mainCharacterIdlist.Count > 0)
                    {
                        foreach (var mainCharacterIds in mainCharacterIdlist)
                        {
                            bool hasCommonItems = mainCharacterIds.Intersect(characterToBeRemoved).Any();
                            if (hasCommonItems) 
                            {
                                throw new Exception($"Update character pool failed. One ore more character is the main character of some sections.");
                            }
                        }
                    }

                    //check if linked to any chapters
                    var totalCh = await _chapterCollection.AsQueryable()
                        .Where(x=>x.BookId==id && x.CharacterIdList != null && x.CharacterIdList.Count>0)
                        .Select(c=> new ChapterView
                        {
                            ChapterId = c.ChapterId,
                            CharacterIdList = c.CharacterIdList
                        }).ToListAsync();

                    if (totalCh.Count > 0) 
                    {
                        List<List<int>> allIdList = totalCh.Where(x=>x.CharacterIdList !=null && x.CharacterIdList.Count>0).Select(x => x.CharacterIdList).ToList();

                        if (allIdList.Count > 0)
                        {
                            foreach (var idList in allIdList)
                            {
                                bool hasCommonItems = idList.Intersect(characterToBeRemoved).Any();
                                if (hasCommonItems)
                                {
                                    throw new Exception($"Update character pool failed. One ore more character is linked to this book's chapter(s).");
                                }
                            }
                        }
                    }

                }

                UpdateDefinition<Book>? characterPoolUpdate = null;    
                List<CharacterInfo> charInfoList = new();
                if (bookView.CharacterPool != null && bookView.CharacterPool.Count > 0)
                {
                    foreach (var item in bookView.CharacterPool)
                    {
                        var charInfo = ConvertDTOToCharacterInfo(item);
                        charInfoList.Add(charInfo);
                    }
                    characterPoolUpdate = Builders<Book>.Update.Set(b => b.CharacterPool, charInfoList);
                }
                else if (bookView.CharacterPool == null || bookView.CharacterPool.Count == 0)
                {
                    characterPoolUpdate = Builders<Book>.Update.Unset(b => b.CharacterPool);
                }

                var result = await _bookCollection.UpdateOneAsync(filter, characterPoolUpdate);
                await _bookCollectionReader.UpdateOneAsync(filter, characterPoolUpdate);

                if (!result.IsAcknowledged)
                {
                    throw new Exception($"error on update book({id}) character pool, in book service, database not process the request");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error on update book({id}) character pool, in book service: {ex}");
            }
        }


        public async Task DeleteBook(int id)
        {
            try
            {
                //remove this id from book sequence list
                List<int> existedSeq = await _seqService.GetBookSequence();
                if (!existedSeq.Contains(id))
                {
                    throw new Exception($"error on DeleteBook, this id {id} NOT in book_seq");
                }
                else
                {
                    existedSeq.Remove(id);
                    await _seqService.UpdateBookSequence(existedSeq);
                    await _bookCollection.DeleteOneAsync(b => b.BookId == id);
                    await _bookCollectionReader.DeleteOneAsync(b => b.BookId == id);
                    await _commentService.DeleteRelatedCommentsByBookSecChIds(id,null,null);
                    await _chapterService.DeleteChapterByBookSectionId(id, null);
                    await _seqService.DeleteBookChapterSeqByBookId(id);

                }

            }
            catch (Exception ex)
            {
                throw new Exception($"error on delete book({id}), in book service: {ex}");
            }
        }


        //for create
        private Book ConvertDTOToBook(BookView bookView)
        {
            List<CharacterInfo>? charInfoList = new();
            if (bookView.CharacterPool != null)
            {
                //check if there is duplicated charachterIdInList
                List<int> charIds = bookView.CharacterPool.Select(x => x.IdInList).ToList();
                var duplicatedId = charIds.GroupBy(x => x).Where(c => c.Count() > 1).Select(a => a.Key).ToList();
                if (duplicatedId.Any())
                {
                    throw new Exception($"Fail to convert book DTO. Duplicated character id in characteralist: {string.Join(',', duplicatedId)}");
                }

                foreach (var item in bookView.CharacterPool)
                {
                    var charInfo = ConvertDTOToCharacterInfo(item);
                    charInfoList.Add(charInfo);
                }
            }

            List<Section> sectionList = new();
            if (bookView.SectionList != null)
            {
                //check if there is duplicated section Id in list
                List<int> secIds = bookView.SectionList.Select(x => x.Id).ToList();
                var duplicatedId = secIds.GroupBy(x => x).Where(c => c.Count() > 1).Select(a => a.Key).ToList();
                if (duplicatedId.Any())
                {
                    throw new Exception($"Fail to convert book DTO. Duplicated section id in characteralist: {string.Join(',', duplicatedId)}");
                }

                foreach (var item in bookView.SectionList)
                {
                    var section = ConvertDTOToSection(item);
                    sectionList.Add(section);
                }
            }

            Book book = new Book
            {
                Title = bookView.Title,
                Introduction = bookView.Introduction,
                Era = bookView.Era,
                Sexuality = bookView.Sexuality,
                TagList = bookView.TagList,
                CharacterPool = bookView.CharacterPool != null ? charInfoList : null,
                SectionList = sectionList,
                IsPublic = bookView.Public,
                IsOriginal = bookView.Original,
                HasMeat = bookView.Meat,
                AllowComment = bookView.AllowComment,
                MaxCommentLength = bookView.MaxCommentLength,
            };
            return book;
        }

        private CharacterInfo ConvertDTOToCharacterInfo(CharacterInfoView characterInfoView)
        {
            return new CharacterInfo
            {
                FirstName = characterInfoView.FirstName,
                LastName = characterInfoView.LastName,
                IdInList = characterInfoView.IdInList,
                LastName_Front = characterInfoView.LastName_Front,
                Description = characterInfoView.Description,

            };
        }

        private Section ConvertDTOToSection(SectionView sectionView)
        {
            return new Section
            {
                Id = sectionView.Id,
                Name = sectionView.Name,
                DisplaySequence = sectionView.DisplaySequence,
                Era = sectionView.Era,
                MainCharacterId = sectionView.MainCharacterId,
                IsPublic = sectionView.Public,
                Description = sectionView.Description,
            };
        }

    }
}
