using BlogA.DAL;
using BlogA.Models;
using BlogA.ModelViews;
using MongoDB.Driver;
using MongoDB.Driver.Linq;


namespace BlogA.Services
{
    public class CommentService
    {
        private IMongoCollection<Book> _bookCollection;
        private IMongoCollection<Chapter> _chapterCollection;
        private IMongoCollection<Comment> _commentCollectionReader;
        private readonly Sequence _seqService;

        private readonly int _usernameLength = 15;
        private int _maxCommentWordCount = 1000;

        public CommentService(DatabaseConnection dbConnection, ReaderDBConnection readerDBConnection, Sequence seqService)
        {
            _bookCollection = dbConnection.Book;
            _seqService = seqService;
            _chapterCollection = dbConnection.Chapter;
            _commentCollectionReader = readerDBConnection.Comment;

        }

        public async Task<List<CommentView>> GetAllComments()
        {
            try
            {
                List<CommentView> cmtList = new();

                cmtList = await _commentCollectionReader.AsQueryable()
                    .Select(c => new CommentView
                    {
                        CommentId = c.CommentId,
                        MainCommentId = c.MainCommentId,
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Content = c.Content,
                        PostDateString = c.PostDateString,
                        ProfileId = c.ProfileId,
                        Username = c.Username,
                        IsViewed = c.IsViewed,
                        Archived = c.Archived,
                        CanBeCommented = c.CanBeCommented,
                        MaxSubCommentLength = c.MaxSubCommentLength,
                        IsAuthor = c.IsAuthor,
                    }).ToListAsync();


                //populate bookTitle, sectionName, chapterHeading
                //all comments have bookId, sectionId, sectionName may be "default"
                //chapterId is optional
                if (cmtList.Count > 0)
                {
                    //get book title, section name
                    var bookIds = cmtList.GroupBy(c => c.BookId).Select(x => x.Key).ToList();
                    var bookFilter = Builders<Book>.Filter.In(b=>b.BookId,bookIds);
                    if (bookIds.Count == 1)
                    {
                        bookFilter = Builders<Book>.Filter.Eq(b => b.BookId, bookIds[0]);
                    }
                    var bookSectionInfo = await _bookCollection.Find(bookFilter).Project(x => new
                    {
                        BookId = x.BookId,
                        Title = x.Title,
                        SectionList = x.SectionList.Select(s => new 
                        {
                            Id = s.Id,
                            Name = s.Name,
                        }).ToList(),
                    }).ToListAsync();

                    if (bookIds.Count != bookSectionInfo.Count) 
                    {
                        throw new Exception($"error on getting comments that associated with book Id: {string.Join(',', bookIds)}, comment may exist for a non-existed book, in comment service:");
                    }

                    foreach (var book in bookSectionInfo)
                    {
                        foreach (var cmt in cmtList)
                        {

                            if (cmt.BookId == book.BookId)
                            {
                                cmt.BookTitle = book.Title;
                                cmt.SectionName = book.SectionList.Where(s => s.Id == cmt.SectionId).Select(s => s.Name).FirstOrDefault();
                            }
                        }
                    }

                    //get chapter heading 
                    var chapterIds = cmtList.Where(c=>c.ChapterId!=null).GroupBy(c => c.ChapterId).Select(x => x.Key).ToList();
                    if (chapterIds.Any())
                    {
                        //get chapter id, sec id, name
                        var chFilter = Builders<Chapter>.Filter.In(c => c.ChapterId, chapterIds);
                        if (chapterIds.Count == 1)
                        {
                            chFilter = Builders<Chapter>.Filter.Eq(c => c.ChapterId, chapterIds[0]);
                        }
                        var relatedCh = await _chapterCollection.Find(chFilter).Project(x => new
                        {
                            x.ChapterId,
                            x.SectionId,
                            x.Heading
                        }).ToListAsync();

                        if(relatedCh.Count==0 )
                        {
                            throw new Exception($"error on getting comments that associated with specific chapter Id: {string.Join(',',chapterIds)}, comment existed for a non-existed chapter, in comment service:");
                        }

                        //chapter may be relocated to other section, so comment shall update the secId too
                        List<CommentView> cmtToBeUpdatedSecId = new();

                        foreach (var ch in relatedCh)
                        {
                            foreach (var cmt in cmtList)
                            {
                                if (cmt.ChapterId !=null && cmt.ChapterId==ch.ChapterId)
                                {
                                    cmt.ChapterHeading = ch.Heading;
                                    if (cmt.SectionId != ch.SectionId) 
                                    {
                                        cmt.SectionId = ch.SectionId;
                                        var targetSec = bookSectionInfo.Where(b=>b.BookId==cmt.BookId).Select(s=>s.SectionList).FirstOrDefault();
                                        cmt.SectionName = targetSec.Where(x=>x.Id==cmt.SectionId).Select(s=>s.Name).FirstOrDefault();
                                        cmtToBeUpdatedSecId.Add(cmt);
                                    }
                                }
                            }
                        }
                        if (cmtToBeUpdatedSecId.Count > 0)
                        {
                            foreach(var cmt in cmtToBeUpdatedSecId)
                            {
                                await UpdateComment(cmt);
                            }
                        }

                    }



                }
                return cmtList;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting all comments, in comment service: {ex}");
            }
        }

        public async Task<List<CommentView>> GetCommentByBookId(int bookId)
        {

            var existed = await _bookCollection.Find(b => b.BookId == bookId).FirstOrDefaultAsync();
            if (existed == null)
            {
                throw new Exception($"Cannot get comments because book({bookId}) is not in the system.");
            }

            try
            {
                List<CommentView> cmtList = new();

                cmtList = await _commentCollectionReader.AsQueryable()
                    .Where(x => x.BookId == bookId)
                    .Select(c => new CommentView
                    {
                        CommentId = c.CommentId,
                        MainCommentId = c.MainCommentId,
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Content = c.Content,
                        PostDateString = c.PostDateString,
                        ProfileId = c.ProfileId,
                        Username = c.Username,
                        IsViewed = c.IsViewed,
                        Archived = c.Archived,
                        CanBeCommented = c.CanBeCommented,
                        MaxSubCommentLength = c.MaxSubCommentLength,
                        IsAuthor = c.IsAuthor,
                    }).ToListAsync();


                //populate bookTitle, sectionName, chapterHeading
                //all comments have bookId, sectionId, sectionName may be "default"
                //chapterId is optional
                if (cmtList.Count > 0)
                {
                    var bookSectionInfo = await _bookCollection.AsQueryable()
                        .Where(b => b.BookId == bookId)
                        .Select(x => new BookView
                        {
                            Title = x.Title,
                            SectionList = x.SectionList.Select(s => new SectionView
                            {
                                Id = s.Id,
                                Name = s.Name,
                            }).ToList(),
                        }).FirstOrDefaultAsync();

                    foreach (var cmt in cmtList)
                    {
                        if (bookSectionInfo != null)
                        {
                            cmt.BookTitle = bookSectionInfo.Title;
                            cmt.SectionName = bookSectionInfo.SectionList.Where(s => s.Id == cmt.SectionId).Select(s => s.Name).FirstOrDefault();
                        }
                    }

                    //get chapter heading 
                    var chapterIds = cmtList.GroupBy(c => c.ChapterId).Select(x => x.Key).ToList();
                    if (chapterIds.Any())
                    {
                        foreach (var chapterId in chapterIds)
                        {
                            string? heading = await GetHeadingByChapterId(chapterId);

                            foreach (var cmt in cmtList)
                            {
                                if (heading != null && cmt.ChapterId == chapterId)
                                {
                                    cmt.ChapterHeading = heading;
                                }
                            }
                        }
                    }
                }
                return cmtList;

            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting all comments for book({bookId}), in comment service: {ex}");
            }
        }

        //get one comment by comment id (search by username, profileId, postDate can be done in front-end)
        public async Task<CommentView> GetCommentById(int cmtId)
        {
            try
            {
                CommentView comment = new CommentView();

                comment = await _commentCollectionReader.AsQueryable()
                    .Where(x => x.CommentId == cmtId)
                    .Select(c => new CommentView
                    {
                        CommentId = c.CommentId,
                        MainCommentId = c.MainCommentId,
                        BookId = c.BookId,
                        SectionId = c.SectionId,
                        ChapterId = c.ChapterId,
                        Content = c.Content,
                        PostDateString = c.PostDateString,
                        ProfileId = c.ProfileId,
                        Username = c.Username,
                        IsViewed = c.IsViewed,
                        Archived = c.Archived,
                        CanBeCommented = c.CanBeCommented,
                        MaxSubCommentLength = c.MaxSubCommentLength,
                        IsAuthor = c.IsAuthor,
                    }).FirstOrDefaultAsync();


                if (comment != null)
                {
                    var bookSectionInfo = await _bookCollection.AsQueryable()
                        .Where(b => b.BookId == comment.BookId)
                        .Select(x => new BookView
                        {
                            Title = x.Title,
                            SectionList = x.SectionList.Select(s => new SectionView
                            {
                                Id = s.Id,
                                Name = s.Name,
                            }).ToList(),
                        }).FirstOrDefaultAsync();

                    if (bookSectionInfo != null)
                    {
                        comment.BookTitle = bookSectionInfo.Title;
                        comment.SectionName = bookSectionInfo.SectionList.Where(s => s.Id == comment.SectionId).Select(s => s.Name).FirstOrDefault();

                        //get chapter heading 
                        if (comment.ChapterId != null)
                        {
                            string? heading = await GetHeadingByChapterId(comment.ChapterId);

                            if (heading != null) comment.ChapterHeading = heading;
                        }
                    }
                }
                return comment;

            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting comment({cmtId}), in comment service: {ex}");
            }
        }

        //create a comment from author side - isAuthor is true automatically
        public async Task<int> CreateComment(CommentView commentView)
        {
            try
            {
                List<Exception> errorList = new();
                //check if book existed 
                var book = await _bookCollection.AsQueryable()
                    .Where(x => x.BookId == commentView.BookId)
                    .Select(b => new BookView
                    {
                        Public = b.IsPublic,
                        AllowComment = b.AllowComment,
                        MaxCommentLength = b.MaxCommentLength,
                        SectionList = b.SectionList.Select(s => new SectionView
                        {
                            Id = s.Id,
                            Public = s.IsPublic
                        }).ToList(),
                    }).FirstOrDefaultAsync();
                if (book == null)
                {
                    throw new Exception("Cannot comment to a book that not in the system.");
                }
                else
                {
                    if (commentView.Username.Length > _usernameLength)
                    {
                        throw new Exception($"Username exceeds max length: {_usernameLength}.");
                    }

                    //Reader role has additional restrictions
                    if (!commentView.IsAuthor)
                    {
                        //check book settings
                        if (!book.Public || !book.AllowComment)
                        {
                            throw new Exception("Cannot comment on book that is not shown to public, or its setting is not allowed to comment.");
                        }

                        var existedBookCmts = await GetCommentByBookId(commentView.BookId);
                        if (existedBookCmts.Count >= book.MaxCommentLength)
                        {
                            throw new Exception("Cannot comment on book as it already exceeds max comment quantity.");
                        }
                    }

                    //apply to both author/reader
                    //to existed section that its isPublic==true 
                    var targetSection = book.SectionList.Where(s => s.Id == commentView.SectionId).FirstOrDefault();
                    if (targetSection == null || !targetSection.Public)
                    {

                        throw new Exception($"Cannot comment on book({commentView.BookId}) - Section({commentView.SectionId}). The section is invalid or not public.");
                    }

                    //if mainCommentId is provided, check if it is valid                   
                    CommentView mainCmt;
                    if (commentView.MainCommentId != -1)
                    {
                        //apply to both author / reader
                        mainCmt = await _commentCollectionReader.AsQueryable()
                           .Where(x => x.CommentId == commentView.MainCommentId)
                           .Select(c => new CommentView
                           {
                               MainCommentId = c.MainCommentId,
                               CanBeCommented = c.CanBeCommented,
                               MaxSubCommentLength = c.MaxSubCommentLength,
                               Archived = c.Archived,
                           }).FirstOrDefaultAsync();

                        if (mainCmt == null)
                        {
                            throw new Exception($"Cannot comment on a non existed comment.");
                        }
                        if (mainCmt.MainCommentId != -1)
                        {
                            errorList.Add(new Exception("Cannot comment on a sub comment."));
                        }

                        //Reader role - check main comment settings
                        if (!commentView.IsAuthor)
                        {
                            if (mainCmt.Archived)
                            {
                                errorList.Add(new Exception($"Cannot comment on an archived comment({mainCmt.CommentId})."));
                            }
                            if (!mainCmt.CanBeCommented)
                            {
                                errorList.Add(new Exception($"This comment({mainCmt.CommentId}) is set to not allowed to comment."));
                            }
                            var subCmts = await _commentCollectionReader.AsQueryable()
                                .Where(x => x.MainCommentId != -1 && x.MainCommentId == commentView.MainCommentId)
                                .Select(s => s.CommentId).ToListAsync();

                            if (subCmts.Count >= mainCmt.MaxSubCommentLength)
                            {
                                errorList.Add(new Exception($"Cannot comment on this comment({mainCmt.CommentId}) as it already reaches its max comment quantity."));
                            }
                        }


                        //check the comment content, null or exceed 300 words
                        if (commentView.Content.Count == 0 || string.IsNullOrWhiteSpace(string.Join("", commentView.Content)))
                        {
                            errorList.Add(new Exception("Cannot post comment while provided content is empty."));
                        }
                        //max word could be set by Miscellaneous/General setting
                        //front-end will handle the word count limited, 200, 500 ect.
                        //but for sure the comment could not exceed 1000
                        if (Helper.CalculateWordCount(commentView.Content) > _maxCommentWordCount)
                        {
                            errorList.Add(new Exception("Cannot post comment while provided content exceeds max word count limit."));
                        }

                        //throw the errorList if there is any
                        if (errorList.Count > 0)
                        {
                            throw new AggregateException("Post comment failed. Please check the error messages.", errorList);
                        }
                    }

                    #region insert a comment
                    if (commentView.IsAuthor)
                    {
                        commentView.IsViewed = true;
                    }

                    Comment comment = ConvertDTOToComment(commentView);
                    int nextId = await _seqService.GetCommentId();
                    comment.CommentId = nextId;
                    await _commentCollectionReader.InsertOneAsync(comment);
                    #endregion

                    return comment.CommentId;
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"error on post comment, in comment service: {ex}");
            }
        }

        //update a comment, can only modify the isAuthor, isViewed, Archived, canBeCommented, MaxSubComment length
        public async Task UpdateComment(CommentView commentView)
        {
            try
            {
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, commentView.CommentId);
                var update = Builders<Comment>.Update
                    .Set(c => c.SectionId, commentView.SectionId)
                    .Set(c => c.IsAuthor, commentView.IsAuthor)
                    .Set(c => c.IsViewed, commentView.IsViewed)
                    .Set(c => c.Archived, commentView.Archived)
                    .Set(c => c.CanBeCommented, commentView.CanBeCommented)
                    .Set(c => c.MaxSubCommentLength, commentView.MaxSubCommentLength);

                await _commentCollectionReader.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on update comment({commentView.CommentId}), in comment service: {ex}");
            }
        }

        public async Task UpdateMultipleComments(List<CommentView> commentViews)
        {
            try
            {
                //send each one to the method above? 
                if (commentViews.Count > 0)
                {
                    foreach (var cv in commentViews)
                    {
                        await UpdateComment(cv);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error on update multiple comments, in comment service: {ex}");
            }
        }


        public async Task DeleteCommentById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new Exception($"Cannot delete comment by invalid ID({id}) provided.");
                }

                //can keep this comment's subComment
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, id);
                await _commentCollectionReader.DeleteOneAsync(filter);

            }
            catch (Exception ex)
            {
                throw new Exception($"error on delete comment({id}), in comment service: {ex}");
            }
        }


        //delete multiple comments by comment ids
        public async Task DeleteByMultipleCommentIds(List<int> ids)
        {
            try
            {
                if (!ids.Any())
                {
                    throw new Exception("no id list provided, in CommentService -  delete multiple comment");
                }
                if (ids.Any(x => x <= 0))
                {
                    throw new Exception($"in CommentService -  delete multiple comment. One ore more invalid ID is in the provided IDs. Your provided IDs: {string.Join(",", ids)}");
                }
                if (ids.Count == 1)
                {
                    //direct to DeleteComment because MondoDB cannot use Filter.In when there is only 1 item in list
                    await DeleteCommentById(ids[0]);
                    return;
                }

                var filter = Builders<Comment>.Filter.In(c => c.CommentId, ids);
                await _commentCollectionReader.DeleteManyAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"error on delete multiple comments, in comment service: {ex}");
            }
        }

        //when book, section, chapter were deleted
        //this method will be called from bookService's update/delete, chapterService's delete
        public async Task DeleteRelatedCommentsByBookSecChIds(int? bookId, List<int>? sectionIds, List<int>? chapterIds)
        {
            try
            {
                FilterDefinition<Comment> filter = Builders<Comment>.Filter.Eq(c => c.BookId, bookId);

                //delete by book id
                if (bookId != null && sectionIds == null && chapterIds == null)
                {
                    await _commentCollectionReader.DeleteManyAsync(filter);
                    return;
                }

                //delete by book, section ids
                if (bookId != null && sectionIds != null && chapterIds == null)
                {
                    if (sectionIds.Count == 1)
                    {
                        filter = Builders<Comment>.Filter.Eq(c => c.BookId, bookId) & Builders<Comment>.Filter.Eq(c => c.SectionId, sectionIds[0]);
                    }
                    if (sectionIds.Count > 1)
                    {
                        filter = Builders<Comment>.Filter.Eq(c => c.BookId, bookId) & Builders<Comment>.Filter.In(c => c.SectionId, sectionIds);
                    }
                    await _commentCollectionReader.DeleteManyAsync(filter);
                    return;
                }

                //delete by chapter ids
                if (bookId == null && sectionIds == null && chapterIds != null)
                {
                    var cmtIds = await _commentCollectionReader.AsQueryable()
                        .Where(x => x.ChapterId != null && chapterIds.Contains((int)x.ChapterId))
                        .Select(c => c.CommentId).ToListAsync();

                    if (cmtIds.Count > 0)
                    {
                        if (cmtIds.Count == 1)
                        {
                            filter = Builders<Comment>.Filter.Eq(c => c.CommentId, cmtIds[0]);
                        }
                        if (cmtIds.Count > 1)
                        {
                            filter = Builders<Comment>.Filter.In(c => c.CommentId, cmtIds);
                        }
                        await _commentCollectionReader.DeleteManyAsync(filter);
                    }
                    return;
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"error on deleting multiple comments, in comment service: {ex}");
            }
        }

        private Comment ConvertDTOToComment(CommentView commentView)
        {
            //Remove tailing white lines
            commentView.Content = Helper.RemoveTailingLines(commentView.Content);
            Comment comment = new Comment()
            {
                BookId = commentView.BookId,
                SectionId = commentView.SectionId,
                Content = commentView.Content,
                PostDateString = commentView.PostDateString,
                ProfileId = commentView.ProfileId,
                Username = commentView.Username,
                IsViewed = commentView.IsViewed,
                Archived = commentView.Archived,
                CanBeCommented = commentView.CanBeCommented,
                MaxSubCommentLength = commentView.MaxSubCommentLength,
                IsAuthor = commentView.IsAuthor,
            };

            if (commentView.ChapterId != null)
            {
                comment.ChapterId = commentView.ChapterId;
            }
            if (commentView.MainCommentId != -1)
            {
                comment.MainCommentId = commentView.MainCommentId;
            }
            return comment;

        }

        private async Task<string?> GetHeadingByChapterId(int? chapterId)
        {
            string heading = await _chapterCollection.AsQueryable()
                    .Where(c => c.ChapterId == chapterId)
                    .Select(x => x.Heading).FirstOrDefaultAsync();
            return heading;
        }


    }
}
