using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOAddRecordRequestTest
    {

        [Key]
        public int Id { get; set; }
        public int RecordId { get; set; }
        public List<int> RequestTestId { get; set; }
    }
}
