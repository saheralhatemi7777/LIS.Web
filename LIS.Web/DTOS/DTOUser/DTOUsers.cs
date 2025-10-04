using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOUsers
    {
            [Key]
            public int UserID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public int Role { get; set; }
            public bool IsActive { get; set; }
            public string RoleName { get; set; }
        
    }
}
