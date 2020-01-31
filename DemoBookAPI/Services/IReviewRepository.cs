﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoBookAPI.Services
{
    public interface IReviewRepository
    {
        ICollection<Review> GetReviews();

        Review GetReview(int reviewId);

        ICollection<Review> GetReviewsOfABook(int bookId);

        Book GetBookOfAReview(int reviewId);

        bool ReviewExists(int reviewId);

    }
}
