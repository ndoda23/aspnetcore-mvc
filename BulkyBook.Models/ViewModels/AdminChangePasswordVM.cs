using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class AdminChangePasswordVM
    {
        public string UserId {  get; set; }

        [ValidateNever]
        [Display(Name ="User Email")]
        public string UserEmail {  get; set; }

        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Password must be at least 7 characters.")]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        public string NewPassword {  get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm New Password")]
        [Compare("NewPassword",ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
