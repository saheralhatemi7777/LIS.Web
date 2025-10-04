namespace APiUsers.DTOs
{
    public class DTOAddOperations
    {

        public int OperationId { get; set; }

        //رقم المستخدم الذي قام بالعمليه

        public int UserId { get; set; }
        //اسم العمليه التي قام بها 
        public string ActionType { get; set; }

        public string TableName { get; set; }


        public int RecordId { get; set; }

        public DateTime ActionDate { get; set; }
    }
}
