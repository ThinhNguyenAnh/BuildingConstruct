﻿using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Configuration
{
    public class ProductSystemCategoriesConfiguration : IEntityTypeConfiguration<ProductSystemCategories>
    {
        public void Configure(EntityTypeBuilder<ProductSystemCategories> builder)
        {

                builder.ToTable("ProductSystemCategories");
                builder.HasKey(x => new { x.ProductSystemID, x.CategoriesID });


                builder.HasOne(x => x.Categories).WithMany(x => x.ProductSystemCategories).HasForeignKey(x => x.CategoriesID);
                builder.HasOne(x => x.ProductSystem).WithMany(x => x.ProductSystemCategories).HasForeignKey(x => x.ProductSystemID);
        }

    }
}