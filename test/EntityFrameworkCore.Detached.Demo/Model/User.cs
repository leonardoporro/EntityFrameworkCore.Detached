﻿using EntityFrameworkCore.Detached.Conventions;
using EntityFrameworkCore.Detached.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Detached.Demo.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ManyToMany("UserRoles")]
        public IList<Role> Roles { get; set; }

        public Company Company { get; set; }
    }
}