﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ToyStore.Models
{
    [Table("Producer")]
    public class Producer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { set; get; }

        [MaxLength(256)]
        public string Name { set; get; }

        [MaxLength(256)]
        public string Imfomation { set; get; }

        [MaxLength(500)]
        public string Logo { set; get; }
        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public virtual IEnumerable<Product> Products { set; get; }
        public virtual IEnumerable<ImportCoupon> ImportCoupons { set; get; }
    }
}