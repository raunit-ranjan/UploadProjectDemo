﻿namespace UploadProjectDemo.Models.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime UploadedOn { get; set; }
    }
}
