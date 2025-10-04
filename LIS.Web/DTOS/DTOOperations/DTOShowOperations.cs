namespace مشروع_ادار_المختبرات.DTOS
{
  
        public class DTOShowOperations
        {
            public int OperationId { get; set; }

            //رقم المستخدم الذي قام بالعمليه

            public string UserName { get; set; }
            //اسم العمليه التي قام بها 
            public string ActionType { get; set; }

            public string TableName { get; set; }


            public string PateintName { get; set; }

            public DateTime ActionDate { get; set; }
        
    }

}
