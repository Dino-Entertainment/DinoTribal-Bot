﻿#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games.Services
{
    public class QuizService : TheGodfatherHttpService
    {
        private static readonly string _url = "https://opentdb.com";


        public override bool IsDisabled => false;


        public static async Task<int?> GetCategoryIdAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category missing!", nameof(category));

            category = category.ToLowerInvariant();

            IReadOnlyList<QuizCategory> categories = await GetCategoriesAsync().ConfigureAwait(false);
            QuizCategory result = categories
                ?.OrderBy(c => category.LevenshteinDistance(c.Name.ToLowerInvariant()))
                .FirstOrDefault();

            if (result is null || category.LevenshteinDistance(result.Name.ToLowerInvariant()) > 2)
                return null;

            return result.Id;
        }

        public static async Task<IReadOnlyList<QuizCategory>> GetCategoriesAsync()
        {
            string response = await _http.GetStringAsync($"{_url}/api_category.php").ConfigureAwait(false);
            QuizCategoryList data = JsonConvert.DeserializeObject<QuizCategoryList>(response);
            return data.Categories.AsReadOnly();
        }

        public static async Task<IReadOnlyList<QuizQuestion>> GetQuestionsAsync(int category, int amount = 10, QuestionDifficulty difficulty = QuestionDifficulty.Easy)
        {
            if (category < 0)
                throw new ArgumentException("Category ID is invalid!", nameof(category));

            if (amount < 1 || amount > 20)
                throw new ArgumentException("Question amount out of range (max 20)", nameof(amount));

            string reqUrl = $"{_url}/api.php?amount={amount}&category={category}&difficulty={difficulty.ToAPIString()}&type=multiple&encode=url3986";
            string response = await _http.GetStringAsync(reqUrl).ConfigureAwait(false);
            QuizData data = JsonConvert.DeserializeObject<QuizData>(response);
            if (data.ResponseCode == 0) {
                return data.Questions.Select(q => {
                    q.Content = WebUtility.UrlDecode(q.Content);
                    q.Category = WebUtility.UrlDecode(q.Category);
                    q.CorrectAnswer = WebUtility.UrlDecode(q.CorrectAnswer);
                    q.Difficulty = difficulty;
                    q.IncorrectAnswers = q.IncorrectAnswers.Select(ans => WebUtility.UrlDecode(ans)).ToList();
                    return q;
                }).ToList().AsReadOnly();
            } else {
                return null;
            }
        }

    }
}
