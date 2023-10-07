﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlix.Models.ViewModels
{
    public class ProductCategoryViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        public List<int> SelectedCategories;

        public int totalProductCount;

        public string searchQuery;
    }
}
