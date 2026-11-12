using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [ValidateNever]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Range must be between 1 and 1000!!!")]
        [Display(Name="List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Range must be between 1 and 1000!!!")]
        [Display(Name = "Price for 1-50")]
        public double Price { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Range must be between 1 and 1000!!!")]
        [Display(Name = "Price for 50+")]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Range must be between 1 and 1000!!!")]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }

        [Display(Name ="Category")]
        public int CategoryId {  get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Display(Name = "Product Image")]
        public string? ImageUrl { get; set; }
    }
}
