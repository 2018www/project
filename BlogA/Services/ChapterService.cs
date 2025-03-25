using MongoDB.Driver;
using BlogA.Models;
using BlogA.DAL;
using BlogA.ModelViews;
using MongoDB.Driver.Linq;


namespace BlogA.Services
{
    public class ChapterService
    {
        private IMongoCollection<Book> _bookCollection;
        private IMongoCollection<Book> _bookCollectionReader;
        private IMongoCollection<Chapter> _chapterCollection;
        private IMongoCollection<Chapter> _chapterCollectionReader;
        private readonly Sequence _seqService;
        private readonly Helper _helper;
        private readonly CommentService _commentService;
        private readonly List<string> _actions = new() { "client", "db" };

        public ChapterService(DatabaseConnection dbConnection, ReaderDBConnection readerDBConnection, Sequence seqService, CommentService commentService)
        {
            _chapterCollection = dbConnection.Chapter;
            _chapterCollectionReader = readerDBConnection.Chapter;
            _bookCollection = dbConnection.Book;
            _bookCollectionReader = readerDBConnection.Book;
            _seqService = seqService;
            _helper = new Helper(dbConnection.DbAWord,dbConnection.DbANum, dbConnection.DbBWord, dbConnection.DbBNum);
            _commentService = commentService;
        }


        public async Task<List<ChapterView>> GetAllChapters()
        {
            try
            {
                List<ChapterView> chapterList = new();
                var chapterListRaw = await _chapterCollection.AsQueryable()
                    .Select(c => new ChapterView
                    {
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Heading = c.Heading,
                        Sidenote = c.Sidenote == null ? null : c.Sidenote,
                        AuthorNote = c.AuthorNote == null ? null : c.AuthorNote,
                        AuthorNotePosition = c.AuthorNotePosition == null ? null : c.AuthorNotePosition,
                        CharacterIdList = c.CharacterIdList == null ? null : c.CharacterIdList,
                        PostDateString = c.PostDateString,
                        Public = c.IsPublic,
                        WordCount = c.WordCount,
                    }).ToListAsync();

                List<int> bookIds = chapterListRaw.Select(c => c.BookId).Distinct().ToList(); 
                //populate the sequence for each chapter
                if (chapterListRaw.Count > 0)
                {
                    foreach (var bookId in bookIds)
                    {
                        Dictionary<int, List<int>> secChSeq = await _seqService.GetOneBookChapterSequence(bookId);
                        var relatedChList = chapterListRaw.Where(x=>x.BookId==bookId).Select(c => c).ToList();

                        if (secChSeq.Count == 0)
                        {
                            throw new Exception($"error on getting book({bookId}) chapter sequence, no data in system");
                        }
                        else
                        {
                            foreach (var ch in relatedChList)
                            {
                                //Sec#:[ch1,ch2,ch3]
                                if (secChSeq.ContainsKey(ch.SectionId))
                                {
                                    if (secChSeq[ch.SectionId].Contains(ch.ChapterId))
                                    {
                                        int index = secChSeq[ch.SectionId].IndexOf(ch.ChapterId);
                                        ch.Sequence = index;
                                        chapterList.Add(ch);
                                    }
                                    else
                                    {
                                        throw new Exception($"error on setting book({bookId}) chapter sequence, no such chapter({ch.ChapterId}) in system");
                                    }
                                }
                                else
                                {
                                    throw new Exception($"error on setting book({bookId}) chapter sequence, no such sec({ch.SectionId}) in system");
                                }
                            }
                        }
                    }
                }

                return chapterList;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting all chapters, in chapter service: {ex}");
            }
        }
        public async Task<List<ChapterView>> GetOneBookChapters(int bookId)
        {
            try
            {
                List<ChapterView> chapterList = new();
                chapterList = await _chapterCollection.AsQueryable()
                    .Where(x => x.BookId == bookId)
                    .Select(c => new ChapterView
                    {
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Heading = c.Heading,
                        Sidenote = c.Sidenote == null ? null : c.Sidenote,
                        AuthorNote = c.AuthorNote == null ? null : c.AuthorNote,
                        AuthorNotePosition = c.AuthorNotePosition == null ? null : c.AuthorNotePosition,
                        CharacterIdList = c.CharacterIdList == null ? null : c.CharacterIdList,
                        PostDateString = c.PostDateString,
                        Public = c.IsPublic,
                        WordCount = c.WordCount,
                    }).ToListAsync();

                //populate the sequence for each chapter
                if (chapterList.Count > 0)
                {
                    Dictionary<int, List<int>> secChSeq = await _seqService.GetOneBookChapterSequence(bookId);
                    if (secChSeq.Count == 0)
                    {
                        throw new Exception($"error on getting book({bookId}) chapter sequence, no data in system");
                    }
                    else
                    {
                        foreach (var ch in chapterList)
                        {
                            //Sec#:[ch1,ch2,ch3]
                            if (secChSeq.ContainsKey(ch.SectionId))
                            {
                                if (secChSeq[ch.SectionId].Contains(ch.ChapterId))
                                {
                                    int index = secChSeq[ch.SectionId].IndexOf(ch.ChapterId);
                                    ch.Sequence = index;

                                }
                                else
                                {
                                    throw new Exception($"error on setting book({bookId}) chapter sequence, no such chapter({ch.ChapterId}) in system");
                                }
                            }
                            else
                            {
                                throw new Exception($"error on setting book({bookId}) chapter sequence, no such sec({ch.SectionId}) in system");
                            }
                        }
                    }

                }


                return chapterList;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting all chapters, in chapter service: {ex}");
            }
        }

        public async Task<ChapterView> GetChapterDetailById(int id)
        {
            try
            {
                ChapterView chapter = await _chapterCollection.AsQueryable()
                    .Where(x => x.ChapterId == id)
                    .Select(c => new ChapterView
                    {
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Heading = c.Heading,
                        Content = c.Content,
                        AuthorNote = c.AuthorNote == null ? null : c.AuthorNote,
                        AuthorNotePosition = c.AuthorNotePosition == null ? null : c.AuthorNotePosition,
                        Sidenote = c.Sidenote == null ? null : c.Sidenote,
                        CharacterIdList = c.CharacterIdList == null ? null : c.CharacterIdList,
                        WordCount = c.WordCount,
                        PostDateString = c.PostDateString,
                        Public = c.IsPublic,
                    }).FirstOrDefaultAsync();

                if (chapter != null)
                {
                    chapter.Content = ConvertContent(_actions[0], chapter.Content, _helper.VA);
                }

                return chapter;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting the chapter({id}), in chapter service: {ex}");
            }
        }


        public async Task<List<string>> GetChapterContentById(int id)
        {
            try
            {
                List<string> content = new();

                var filter = Builders<Chapter>.Filter.Eq(x => x.ChapterId, id);
                var chapterContent = await _chapterCollection.Find(filter).Project(x => new
                {
                    Content = x.Content
                }).FirstOrDefaultAsync();

                if (chapterContent != null)
                {
                    content = chapterContent.Content;
                    if (content.Any())
                    {
                        content = ConvertContent(_actions[0], content, _helper.VA);
                    }
                }
                else
                {
                    throw new Exception($"Error on getting content for chapter {id}, no content exists in the system.");
                }

                return content;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting the chapter({id}), in chapter service: {ex}");
            }
        }
        //post a chapter - will modify the newest chapter in BookCollection, bookId_seq in counterCollection
        public async Task<int> CreateChapter(ChapterView chapterView)
        {
            Dictionary<int, List<int>> existedSecChSeq = await _seqService.GetOneBookChapterSequence(chapterView.BookId);

            bool existedChapters = _chapterCollection.AsQueryable().Any(x => x.BookId == chapterView.BookId);
            if (existedChapters && existedSecChSeq.Count == 0)
            {
                throw new Exception($"post chapter failed, no chapter_seq existed while there are chapters related to book({chapterView.BookId}) in the system");
            }

            //count if there are 999 chapters related to this book
            var filter = Builders<Chapter>.Filter.Eq(x => x.BookId, chapterView.BookId);
            long relatedChCount = await _chapterCollection.CountDocumentsAsync(filter);

            if (relatedChCount >= 999)
            {
                throw new Exception("Post chapter failed. The book already reaches max chapter quantity.");
            }

            var book = await _bookCollection.AsQueryable()
                                .Where(x => x.BookId == chapterView.BookId)
                .Select(b => new BookView
                {
                    SectionList = b.SectionList.Select(s => new SectionView
                    {
                        Id = s.Id,
                    }).ToList(),
                    CharacterPool = b.CharacterPool != null ? b.CharacterPool.Select(c => new CharacterInfoView
                    {
                        IdInList = c.IdInList,
                    }).ToList() : null
                }
                ).FirstOrDefaultAsync();


            //check if the sectionId existed in book
            var sectionIds = book.SectionList.Select(x => x.Id).ToList();
            if (sectionIds.Count == 0 || !sectionIds.Contains(chapterView.SectionId))
            {
                throw new Exception($"post chapter failed, book({chapterView.BookId}) does not have such section id: {chapterView.SectionId}");
            }

            if (book.CharacterPool == null && chapterView.CharacterIdList != null)
            {
                throw new Exception($"post chapter failed, book({chapterView.BookId}) does not have characterPool while chapter contains a characterIdList");
            }

            //check if the character existed in book's characterPool
            if (book.CharacterPool != null && chapterView.CharacterIdList != null)
            {
                var characterIds = book.CharacterPool.Select(x => x.IdInList).ToList();
                foreach (var charId in chapterView.CharacterIdList)
                {
                    if (!characterIds.Contains(charId))
                    {
                        throw new Exception($"post chapter failed, book({chapterView.BookId}) does not have such character({charId}) in characterPool.");
                    }
                }
            }


            if (chapterView.Content == null || chapterView.Content.Count == 0 || string.IsNullOrWhiteSpace(string.Join("", chapterView.Content)))
            {
                throw new Exception($"post chapter failed, no actual content");
            }
            if (chapterView.Heading.Length > 15 || string.IsNullOrWhiteSpace(chapterView.Heading))
            {
                throw new Exception("post chapter failed, heading exceed limitation or it is not provided.");
            }
            if (chapterView.Sidenote != null && !string.IsNullOrWhiteSpace(chapterView.Sidenote))
            {
                if (chapterView.Sidenote.Length > 15)
                {
                    throw new Exception("post chapter failed, sidenote exceed limitation.");
                }
            }
            if (chapterView.AuthorNote != null && chapterView.AuthorNote.Count > 0)
            {
                chapterView.AuthorNote = Helper.RemoveTailingLines(chapterView.AuthorNote);

                if (Helper.CalculateWordCount(chapterView.AuthorNote) > 1000)
                {
                    throw new Exception("post chapter failed, author note exceed limitation.");
                }
            }

            try
            {
                Chapter chapter = ConvertDTOToChapter(chapterView, _helper.VA);

                int nextId = await _seqService.GetChapterId();
                chapter.ChapterId = nextId;
                chapterView.ChapterId = nextId;
                Dictionary<int, List<int>> editedSecChSeq = new Dictionary<int, List<int>>(existedSecChSeq);

                //add the new chapter to the book chapter sequence list, will be last index automatically
                //Sec#:[ch1,ch2,ch3]
                //if this is the 1st chapter of the book / section
                if (existedSecChSeq.Count == 0 || !existedSecChSeq.ContainsKey(chapter.SectionId))
                {
                    List<int> chapterList = new() { chapter.ChapterId };
                    editedSecChSeq[chapter.SectionId] = chapterList;
                }
                else
                {
                    //update existed chapter sequence
                    if (existedSecChSeq.ContainsKey(chapter.SectionId) && !existedSecChSeq[chapter.SectionId].Contains(chapter.ChapterId))
                    {
                        //add to last spot of the list
                        editedSecChSeq[chapter.SectionId].Add(chapter.ChapterId);
                    }
                    else
                    {
                        //roll back if errors
                        await _seqService.CancelChapterId();
                        throw new Exception($"error on setting book({chapter.BookId}) chapter sequence, chapter({chapter.ChapterId}) already in system");
                    }
                }
                await _seqService.UpdateOneBookChapterSequence(chapter.BookId, editedSecChSeq);
                await _chapterCollection.InsertOneAsync(chapter);

                //use another pattern for verB
                Chapter chapterB = ConvertDTOToChapter(chapterView, _helper.VB);
                chapterB.ChapterId = nextId;
                await _chapterCollectionReader.InsertOneAsync(chapterB);

                return chapter.ChapterId;
            }
            catch (Exception ex)
            {
                //roll back if errors
                await _seqService.CancelChapterId();
                await _seqService.UpdateOneBookChapterSequence(chapterView.BookId, existedSecChSeq);
                await _chapterCollection.DeleteOneAsync(c => c.ChapterId == chapterView.ChapterId);
                await _chapterCollectionReader.DeleteOneAsync(c => c.ChapterId == chapterView.ChapterId);
                throw new Exception($"error on create chapter for book({chapterView.BookId}), in chapter service: {ex}");
            }

        }

        public async Task<bool> UpdateChapter(ChapterView chapterView)
        {
            bool success = false;

            if (chapterView.Content == null || chapterView.Content.Count == 0 || string.IsNullOrWhiteSpace(string.Join("", chapterView.Content)))
            {
                throw new Exception($"update chapter({chapterView.ChapterId}) failed, no actual content");
            }
            if (chapterView.Heading.Length > 15 || string.IsNullOrWhiteSpace(chapterView.Heading))
            {
                throw new Exception("update chapter failed, heading exceed limitation or it is not provided.");
            }
            if (chapterView.Sidenote != null && !string.IsNullOrWhiteSpace(chapterView.Sidenote))
            {
                if (chapterView.Sidenote.Length > 15)
                {
                    throw new Exception("update chapter failed, sidenote exceed limitation.");
                }
            }

            try
            {
                Dictionary<int, List<int>> secChSeq = await _seqService.GetOneBookChapterSequence(chapterView.BookId);
                if (secChSeq.Count == 0)
                {
                    throw new Exception($"error on update chapter({chapterView.ChapterId}) for book({chapterView.BookId}) - book chapter sequence is not in system");
                }

                List<string> dbContent = ConvertContent(_actions[1], chapterView.Content, _helper.VA);
                var filter = Builders<Chapter>.Filter.Eq(c => c.ChapterId, chapterView.ChapterId);
                var updateGeneral = Builders<Chapter>.Update
                    .Set(c => c.Heading, chapterView.Heading)
                    .Set(c => c.Content, dbContent)
                    .Set(c => c.WordCount, chapterView.WordCount)
                    .Set(c => c.PostDateString, chapterView.PostDateString)
                    .Set(c => c.IsPublic, chapterView.Public);

                UpdateDefinition<Chapter> updateAuthorNote = null;
                UpdateDefinition<Chapter> updateAuthorNotePosition = null;
                UpdateDefinition<Chapter> updateSideNote = null;
                UpdateDefinition<Chapter> updateCharacterIdList = null;

                if (chapterView.AuthorNote == null || string.IsNullOrWhiteSpace(string.Join("", chapterView.AuthorNote)))
                {
                    updateAuthorNote = Builders<Chapter>.Update.Unset(c => c.AuthorNote);
                    updateAuthorNotePosition = Builders<Chapter>.Update.Unset(c => c.AuthorNotePosition);

                }
                if (chapterView.AuthorNote != null && !string.IsNullOrWhiteSpace(string.Join("", chapterView.AuthorNote)))
                {
                    chapterView.AuthorNote = Helper.RemoveTailingLines(chapterView.AuthorNote);

                    if (Helper.CalculateWordCount(chapterView.AuthorNote) > 1000)
                    {
                        throw new Exception("Update chapter failed, author note exceed limitation.");
                    }

                    updateAuthorNote = Builders<Chapter>.Update.Set(c => c.AuthorNote, chapterView.AuthorNote);
                    updateAuthorNotePosition = Builders<Chapter>.Update.Set(c => c.AuthorNotePosition, chapterView.AuthorNotePosition);

                }

                if (string.IsNullOrWhiteSpace(chapterView.Sidenote))
                {
                    updateSideNote = Builders<Chapter>.Update.Unset(c => c.Sidenote);
                }
                if (!string.IsNullOrWhiteSpace(chapterView.Sidenote))
                {
                    updateSideNote = Builders<Chapter>.Update.Set(c => c.Sidenote, chapterView.Sidenote);
                }

                if (chapterView.CharacterIdList == null || string.IsNullOrWhiteSpace(string.Join("", chapterView.CharacterIdList)))
                {
                    updateCharacterIdList = Builders<Chapter>.Update.Unset(c => c.CharacterIdList);
                }
                if (chapterView.CharacterIdList != null && !string.IsNullOrWhiteSpace(string.Join("", chapterView.CharacterIdList)))
                {
                    updateCharacterIdList = Builders<Chapter>.Update.Set(c => c.CharacterIdList, chapterView.CharacterIdList);
                }

                var updateTotal = Builders<Chapter>.Update.Combine(updateGeneral, updateAuthorNote, updateAuthorNotePosition, updateSideNote, updateCharacterIdList);

                var result = await _chapterCollection.UpdateOneAsync(filter, updateTotal);

                //update reader side 
                dbContent = ConvertContent(_actions[1], chapterView.Content, _helper.VB);
                var updateGeneralReader = Builders<Chapter>.Update
                    .Set(c => c.Heading, chapterView.Heading)
                    .Set(c => c.Content, dbContent)
                    .Set(c => c.WordCount, chapterView.WordCount)
                    .Set(c => c.PostDateString, chapterView.PostDateString)
                    .Set(c => c.IsPublic, chapterView.Public);
                var updateTotalReader = Builders<Chapter>.Update.Combine(updateGeneralReader, updateAuthorNote, updateAuthorNotePosition, updateSideNote, updateCharacterIdList);
                await _chapterCollectionReader.UpdateOneAsync(filter, updateTotalReader);

                if (result.ModifiedCount == 1)
                {
                    success = true;
                }
                if (result.ModifiedCount != 1)
                {
                    throw new Exception($"error on update chapter({chapterView.ChapterId}) for book({chapterView.BookId})");
                }
                return success;

            }
            catch (Exception ex)
            {
                throw new Exception($"error on updating chapter({chapterView.ChapterId}) for book({chapterView.BookId}), in chapter service: {ex}");
            }
        }

        //when a book or book's section is deleted, deleted related chapters
        //call this method from bookService 
        public async Task DeleteChapterByBookSectionId(int bookId, List<int>? sectionIds)
        {
            try
            {
                FilterDefinition<Chapter> filter = Builders<Chapter>.Filter.Eq(c => c.BookId, bookId);

                if (sectionIds != null)
                {
                    if (sectionIds.Count == 1)
                    {
                        filter = Builders<Chapter>.Filter.Eq(c => c.BookId, bookId) & Builders<Chapter>.Filter.Eq(c => c.SectionId, sectionIds[0]);
                    }
                    if (sectionIds.Count > 1)
                    {
                        filter = Builders<Chapter>.Filter.Eq(c => c.BookId, bookId) & Builders<Chapter>.Filter.In(c => c.SectionId, sectionIds);
                    }
                }
                await _chapterCollection.DeleteManyAsync(filter);
                await _chapterCollectionReader.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on deleteChapterByBookSectionId (chapterService), error: {ex}");
            }
        }


        //delete chapters by List<int> - might modify the newest chapter in BookCollection, bookId_seq in counterCollection
        public async Task DeleteOneBookMultipleChapterByIds(int bookId, List<int> chapterIds)
        {
            try
            {
                if (chapterIds.Count == 0)
                {
                    throw new Exception($"error_1 on DeleteOneBookMultipleChapterByIds, no chapterIds provided.");
                }

                //get the book
                var book = await _bookCollection.AsQueryable()
                    .Where(x => x.BookId == bookId)
                    .Select(x => new BookView
                    {
                        BookId = x.BookId,
                        NewestChapterId = x.NewestChapterId
                    }).FirstOrDefaultAsync();
                if (book == null)
                {
                    throw new Exception($"error_2 on DeleteOneBookMultipleChapterByIds, this book({bookId}) is not in the system.");
                }

                //get the chapters
                var chapters = await _chapterCollection.AsQueryable()
                    .Where(x => chapterIds.Contains(x.ChapterId))
                    .Select(c => new ChapterView
                    {
                        BookId = bookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId
                    }).ToListAsync();

                if (!chapters.Any())
                {
                    throw new Exception($"error_3 on DeleteOneBookMultipleChapterByIds, these chapters are not in the system.");
                }
                //check if these chapterIds belong to this book
                if (chapters.Where(x => x.BookId != bookId).Any())
                {
                    List<int> invalidIds = chapters.Where(x => x.BookId != bookId).Select(c => c.ChapterId).ToList();
                    throw new Exception($"error_4 on DeleteOneBookMultipleChapterByIds, one ore more chapters do not belong to book({bookId}). Invalid chapter ids: {string.Join(",", invalidIds)}");
                }

                //get the book_chapter_seq -> sec#:[1,2,3]
                Dictionary<int, List<int>> secChSeq = await _seqService.GetOneBookChapterSequence(bookId);
                if (secChSeq.Count == 0)
                {
                    //a chapter must tight to a valid book chapter seq when posted
                    throw new Exception($"error_5 on DeleteOneBookMultipleChapterByIds, this book({bookId}) chapter sequence is not in the system.");
                }

                //a chapter must tight to a valid book section when posted, so the book_chapter_seq must contain this sec# key
                var secIdsToBeModified = chapters.Select(x => x.SectionId).Distinct().ToList();
                List<int> invalidSecIds = new();
                foreach (var secId in secIdsToBeModified)
                {
                    if (!secChSeq.ContainsKey(secId))
                    {
                        invalidSecIds.Add(secId);
                    }
                }
                if (invalidSecIds.Count > 0)
                {
                    throw new Exception($"error_6 on DeleteOneBookMultipleChapterByIds, this book({bookId}) chapter sequence does not contain this sections: {string.Join(",", invalidSecIds)}.");
                }

                //check if targeted sec_ch_seq (sec#:[1,2,3]) include targeted chapterIds
                List<int> invalidChIds = new();
                Dictionary<int, List<int>> editedSecChSeq = new(secChSeq);
                foreach (var secId in secIdsToBeModified)
                {
                    var relatedChIds = chapters.Where(x => x.SectionId == secId).Select(c => c.ChapterId).ToList();
                    foreach (var chId in relatedChIds)
                    {
                        if (!secChSeq[secId].Contains(chId))
                        {
                            invalidChIds.Add(secId);
                        }
                        else
                        {
                            //remove the targeted chapter Id from that section
                            editedSecChSeq[secId].Remove(chId);
                        }
                    }
                }
                if (invalidChIds.Any())
                {
                    throw new Exception($"error_7 on DeleteOneBookMultipleChapterByIds, this book({bookId}) chapter sequence does not contain these chapterIds in target sections, invalid chapterIds are: {string.Join(",", invalidChIds)}.");
                }

                var filterOne = Builders<Chapter>.Filter.Eq(c => c.BookId, bookId);
                var filterTwo = Builders<Chapter>.Filter.In(c => c.ChapterId, chapterIds);
                if (chapterIds.Count == 1)
                {
                    filterTwo = Builders<Chapter>.Filter.Eq(c => c.ChapterId, chapterIds[0]);
                }
                var combinedFilter = filterOne & filterTwo;

                var result = await _chapterCollection.DeleteManyAsync(combinedFilter);
                await _chapterCollectionReader.DeleteManyAsync(combinedFilter);

                if (result.DeletedCount > 0)
                {
                    //update the book_chapter_seq
                    //remove empty sec#:[]
                    foreach (var secId in secIdsToBeModified)
                    {
                        if (!editedSecChSeq[secId].Any())
                        {
                            editedSecChSeq.Remove(secId);
                        }
                    }

                    if (!editedSecChSeq.Any())
                    {
                        //no chapters left for this book after deleted, so remove the book_chapter_seq
                        await _seqService.DeleteBookChapterSeqByBookId(bookId);

                        //also remove the newest chapter from the book
                        await SetNewestChapter(new List<int> { bookId });

                    }
                    else
                    {
                        await _seqService.UpdateOneBookChapterSequence(bookId, editedSecChSeq);

                        //remove targeted book's newest chapter if the deleted chapters includes the newest chapter                  
                        if (book.NewestChapterId != null)
                        {
                            int newcChId = (int)book.NewestChapterId;
                            if (chapterIds.Contains(newcChId))
                            {
                                //remove the newest chapter from the book
                                await SetNewestChapter(new List<int> { bookId });
                            }
                        }
                    }

                    //delete relatedComment
                    await _commentService.DeleteRelatedCommentsByBookSecChIds(null, null, chapterIds);

                }

            }
            catch (Exception ex)
            {
                throw new Exception($"general errors on DeleteOneBookMultipleChapterByIds: {ex}.");
            }
        }

        //get chapter heading by providing a chapter, call from bookService's getBook
        public async Task<string?> GetHeadingByChapterId(int? chapterId)
        {
            try
            {
                if (chapterId != null && chapterId > 0)
                {
                    var heading = await _chapterCollection.AsQueryable()
                            .Where(x => x.ChapterId == chapterId)
                            .Select(c => c.Heading).FirstOrDefaultAsync();
                    return heading;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw new Exception($"error on getHeadingByChapterId, in ChapterService, error: {ex}");
            }
        }

        public async Task<int> GetNewestChapterBelongSectionId(int? chapterId)
        {
            try
            {
                int secId = -1;
                if (chapterId != null && chapterId > 0)
                {
                    secId = await _chapterCollection.AsQueryable()
                            .Where(x => x.ChapterId == chapterId)
                            .Select(c => c.SectionId).FirstOrDefaultAsync();
                }
                return secId;

            }
            catch (Exception ex)
            {
                throw new Exception($"error on GetNewestChapterBelongSectionId, in ChapterService, error: {ex}");
            }
        }

        //set a chapter as newest chapter in bookCollection
        public async Task SetNewestChapter(List<int> bookChapterIds)
        {
            // bookChapterIds  - {1(bookId), 4(chapterId)}
            // if only bookId - {1}, that means no newest chapter for this book
            try
            {
                int bookId = bookChapterIds[0];
                var filter = Builders<Book>.Filter.Eq(b => b.BookId, bookId);
                UpdateDefinition<Book> update = null;

                if (bookChapterIds.Count == 2)
                {
                    update = Builders<Book>.Update.Set(b => b.NewestChapterId, bookChapterIds[1]);
                }
                else
                {
                    //if there is only bookId, means remove the newest chapter
                    update = Builders<Book>.Update.Unset(b => b.NewestChapterId);
                }
                await _bookCollection.UpdateOneAsync(filter, update);
                await _bookCollectionReader.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on setting book({bookChapterIds[0]}) newest chapter({bookChapterIds[1]}), in chapter service: {ex}");
            }
        }

        //get top 5 chapters
        public async Task<List<ChapterView>> GetTopFiveChapters()
        {
            try
            {
                List<ChapterView> top5Ch = new();
                var originalTop5ChSeq = await _seqService.GetTopFiveChapterSequence();
                List<int> editedTop5ChSeq = originalTop5ChSeq.ToList();

                if (originalTop5ChSeq.Count > 0)
                {
                    var filter = Builders<Chapter>.Filter.In(c => c.ChapterId, originalTop5ChSeq);
                    if (originalTop5ChSeq.Count == 1)
                    {
                        filter = Builders<Chapter>.Filter.Eq(c => c.ChapterId, originalTop5ChSeq[0]);
                    }
                    var existedCh = await _chapterCollection.Find(filter)
                        .Project(c => new
                        {
                            BookId = c.BookId,
                            SectionId = c.SectionId,
                            ChapterId = c.ChapterId,
                            Heading = c.Heading,
                            PostDateString = c.PostDateString,
                            IsPublic = c.IsPublic,
                        }).ToListAsync();


                    if (existedCh.Count > 0)
                    {
                        var targetBookIds = existedCh.Select(c => c.BookId).Distinct().ToList();
                        var bookFilter = Builders<Book>.Filter.In(b => b.BookId, targetBookIds);
                        if (targetBookIds.Count == 1)
                        {
                            bookFilter = Builders<Book>.Filter.Eq(b => b.BookId, targetBookIds[0]);
                        }
                        var existedBook = await _bookCollection.Find(bookFilter)
                            .Project(b => new
                            {
                                BookId = b.BookId,
                                SectionList = b.SectionList,
                                Title = b.Title,
                                IsPublic = b.IsPublic,
                            }).ToListAsync();

                        if (existedBook.Count > 0)
                        {
                            foreach (var ch in existedCh)
                            {

                                var targetBook = existedBook.Where(x => x.BookId == ch.BookId).Select(b => b).FirstOrDefault();

                                if (targetBook != null)
                                {
                                    var targetSection = targetBook.SectionList.Where(x => x.Id == ch.SectionId).Select(s => s).FirstOrDefault();
                                    if (targetSection != null)
                                    {
                                        //if book, section, chapter's public==false, then this chapter's public is False
                                        bool upperlevelPublic = targetBook.IsPublic && targetSection.IsPublic;
                                        bool chPublic = upperlevelPublic && ch.IsPublic;

                                        ChapterView chView = new ChapterView()
                                        {
                                            BookId = ch.BookId,
                                            BookTitle = targetBook.Title,
                                            SectionId = ch.SectionId,
                                            SectionName = targetSection.Name,
                                            ChapterId = ch.ChapterId,
                                            Heading = ch.Heading,
                                            PostDateString = ch.PostDateString,
                                            Public = chPublic,
                                        };

                                        int indexInTop5 = originalTop5ChSeq.IndexOf(ch.ChapterId);
                                        if (indexInTop5 != -1)
                                        {
                                            chView.Sequence = indexInTop5;
                                        }

                                        top5Ch.Add(chView);
                                    }
                                    else
                                    {
                                        throw new Exception($"error on getting top 5 chapters's section, chapter({ch.ChapterId}) existed but not tight to book({ch.BookId})'s target section({ch.SectionId})");
                                    }
                                }
                                else
                                {
                                    throw new Exception($"error on getting top 5 chapters's book, chapter({ch.ChapterId}) existed but not tight to any books in the system");
                                }
                            }

                        }
                        else
                        {
                            throw new Exception($"error on getting top 5 chapters's book, chapters - {string.Join(',', originalTop5ChSeq)} existed but not tight to any books in the system");
                        }

                        //check if any chapterId in Top5 is not valid
                        var existedChIds = existedCh.Select(x => x.ChapterId).ToList();
                        var nonExistedChIds = originalTop5ChSeq.Where(x => !existedChIds.Contains(x)).ToList();
                        if (nonExistedChIds.Count > 0)
                        {
                            await _seqService.UpdateTopFiveChapterSequence(existedChIds);

                        }

                    }
                    else
                    {
                        //remove the non-existed chapter(may be deleted by authors) from top 5 chapter seq 
                        editedTop5ChSeq.Clear();
                        await _seqService.UpdateTopFiveChapterSequence(editedTop5ChSeq);
                    }
                }
                return top5Ch;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting top 5 chapters, in chapter service: {ex}");
            }
        }

        //update top 5 chapter
        public async Task UpdateTopFiveChapters(List<int> chIds)
        {
            try
            {

                bool duplicatedId = chIds.GroupBy(x => x).Any(s => s.Count() > 1);
                if (duplicatedId || chIds.Count > 5)
                {
                    throw new Exception($"Update top 5 chapters sequence failed. Duplicated id in new sequence list, or id exceeds quantity limitations: {string.Join(',', chIds)}");
                }

                if (chIds.Count > 0)
                {
                    //check if all chapter existed, or match the collection
                    var filter = Builders<Chapter>.Filter.In(c => c.ChapterId, chIds);
                    if (chIds.Count == 1)
                    {
                        filter = Builders<Chapter>.Filter.Eq(c => c.ChapterId, chIds[0]);
                    }
                    var docCount = await _chapterCollection.CountDocumentsAsync(filter);
                    long qty = chIds.Count;
                    if (docCount != qty)
                    {
                        throw new Exception($"Update top 5 chapters sequence failed. Provided chapter id(s) do not match the data in the system: {string.Join(',', chIds)}");
                    }
                }

                await _seqService.UpdateTopFiveChapterSequence(chIds);

            }
            catch (Exception ex)
            {
                throw new Exception($"error on updating top 5 chapters, in chapter service: {ex}");
            }
        }


        //update one book chapters seq
        public async Task UpdateOneBookChapterSeq(int bookId, Dictionary<int, List<int>> newSecChSeq)
        {
            //int bookId, Dictionary< int, List<int> > newSecChSeq
            try
            {
                //update each chapter's sec#, and arrange sequence if necessay
                //check against the original book_ch_seq that if the chapter has been switched section, if so then update the sectionId
                Dictionary<int, List<int>> originalSecChSeq = await _seqService.GetOneBookChapterSequence(bookId);

                //destructure the newSeq to Dict<chId, SecId>
                Dictionary<int, int> newChSec = new();
                foreach (var item in newSecChSeq)
                {
                    foreach (var chId in item.Value)
                    {
                        newChSec[chId] = item.Key;
                    }
                }

                //destructure the originalSeq to Dict<chId, SecId>
                Dictionary<int, int> oldChSec = new();
                foreach (var item in originalSecChSeq)
                {
                    foreach (var chId in item.Value)
                    {
                        oldChSec[chId] = item.Key;
                    }
                }

                //compare and pick up the chapters that were been switched section
                Dictionary<int, List<int>> secChIdsToBeUpdated = new();
                foreach (var cs in newChSec)
                {
                    if (newChSec[cs.Key] != oldChSec[cs.Key])
                    {
                        int chapterId = cs.Key;
                        int sectionId = cs.Value;

                        if (!secChIdsToBeUpdated.ContainsKey(sectionId))
                        {
                            List<int> chIdList = new() { chapterId };
                            secChIdsToBeUpdated[sectionId] = chIdList;
                        }
                        else
                        {
                            secChIdsToBeUpdated[sectionId].Add(chapterId);
                        }
                    }
                }
                if (secChIdsToBeUpdated.Count > 0)
                {
                    foreach (var item in secChIdsToBeUpdated)
                    {
                        await UpdateChapterSectionId(bookId, item.Key, item.Value);
                    }
                }

                await _seqService.UpdateOneBookChapterSequence(bookId, newSecChSeq);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on setting book({bookId}) chapter sequence, in chapter service: {ex}");
            }
        }

        private async Task UpdateChapterSectionId(int bookId, int sectionId, List<int> chapterIds)
        {
            try
            {
                if (chapterIds.Count == 0)
                {
                    throw new Exception($"Update chapter sectionId({sectionId}) failed, no chapterIds provided.");
                }

                //check if the book has this section
                var sections = await _bookCollection.AsQueryable()
                    .Where(x => x.BookId == bookId)
                    .Select(s => s.SectionList).FirstOrDefaultAsync();
                if (!sections.Any(x => x.Id == sectionId))
                {
                    throw new Exception($"Update chapter sectionId({sectionId}) failed, no such sectionId({sectionId}) in book({bookId}).");
                }


                var filter = Builders<Chapter>.Filter.In(c => c.ChapterId, chapterIds);
                var update = Builders<Chapter>.Update.Set(c => c.SectionId, sectionId);
                if (chapterIds.Count == 1)
                {
                    filter = Builders<Chapter>.Filter.Eq(c => c.ChapterId, chapterIds[0]);
                    await _chapterCollection.UpdateOneAsync(filter, update);
                    await _chapterCollectionReader.UpdateOneAsync(filter, update);
                    return;
                }
                await _chapterCollection.UpdateManyAsync(filter, update);
                await _chapterCollectionReader.UpdateManyAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on setting chapter -{string.Join(",", chapterIds)}, section({sectionId}), in chapter service: {ex}");
            }
        }

        private Chapter ConvertDTOToChapter(ChapterView chapterView, string version)
        {
            var chapter = new Chapter
            {
                BookId = chapterView.BookId,
                SectionId = chapterView.SectionId,
                Heading = chapterView.Heading,
                Content = chapterView.Content,
                WordCount = chapterView.WordCount,
                IsPublic = chapterView.Public,
                PostDateString = chapterView.PostDateString,
            };

            if (version == _helper.VA)
            {
                chapter.Content = ConvertContent(_actions[1], chapter.Content, _helper.VA);
            }
            if (version == _helper.VB)
            {
                chapter.Content = ConvertContent(_actions[1], chapter.Content, _helper.VB);
            }

            if (chapterView.AuthorNote != null)
            {
                chapter.AuthorNote = chapterView.AuthorNote;
                chapter.AuthorNotePosition = chapterView.AuthorNotePosition;
            }
            if (chapterView.Sidenote != null)
            {
                chapter.Sidenote = chapterView.Sidenote;
            }
            if (chapterView.CharacterIdList != null)
            {
                chapter.CharacterIdList = chapterView.CharacterIdList;
            }

            return chapter;
        }

        private List<string> ConvertContent(string action, List<string> content, string version)
        {
            try
            {
                List<string> afterConvert = new();
                content = Helper.RemoveTailingLines(content);

                if (action == _actions[0])
                {
                    foreach (string line in content)
                    {
                        string after = line;
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (version == _helper.VA)
                            {
                                after = _helper.ConvertConToWord(line);
                            }
                            if (version == _helper.VB)
                            {
                                after = _helper.ConvertConToWordReader(line);
                            }
                        }
                        afterConvert.Add(after);
                    }
                }
                else
                {
                    foreach (string line in content)
                    {
                        string after = line;
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (version == _helper.VA)
                            {
                                after = _helper.ConvertWordToCon(line);
                            }
                            if (version == _helper.VB)
                            {
                                after = _helper.ConvertWordToConReader(line);
                            }
                        }
                        afterConvert.Add(after);
                    }
                }
                return afterConvert;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on convert content - {action}: {ex}");
            }

        }

    }
}

