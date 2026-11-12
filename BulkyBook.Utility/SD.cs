using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Utility
{
    public class SD
    {
        public const string RoleCustomer = "Customer";
        public const string RoleAdmin  = "Admin";
        public const string RoleEmployee = "Employee";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusReFunded = "Refunded";

        public const string SessionCart = "SessionShoppingCartBulkyBook";

        // Audit actions
        public const string AuditCreated = "Created";
        public const string AuditUpdated = "Updated";
        public const string AuditDeleted = "Deleted";
        public const string AuditStatusChanged = "StatusChanged";
        public const string AuditUserLocked = "UserLocked";
        public const string AuditUserUnlocked = "UserUnlocked";
        public const string AuditRoleChanged = "RoleChanged";
        public const string AuditPasswordReset = "PasswordReset";

        // Audit entities
        public const string EntityOrder = "Order";
        public const string EntityUser = "User";
        public const string EntityProduct = "Product";
        public const string EntityCategory = "Category";
    }
}
