﻿namespace AvroConvertTests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvroConvert.Attributes;

    [Equals]
    public class User
    {
        public string name { get; set; }

        public int? favorite_number { get; set; }

        public string favorite_color { get; set; }
    }

    [Equals]
    public class SomeTestClass
    {
        public NestedTestClass objectProperty { get; set; }

        public int simpleProperty { get; set; }
    }

    [Equals]
    public class NestedTestClass
    {
        public string justSomeProperty { get; set; }

        public long andLongProperty { get; set; }
    }

    [Equals]
    public class SmallerNestedTestClass
    {
        public string justSomeProperty { get; set; }
    }

    [Equals]
    public class ClassWithConstructorPopulatingProperty
    {
        public List<NestedTestClass> nestedList { get; set; }
        public List<ClassWithSimpleList> anotherList { get; set; }
        public string stringProperty { get; set; }

        public ClassWithConstructorPopulatingProperty()
        {
            nestedList = new List<NestedTestClass>();
            anotherList = new List<ClassWithSimpleList>();
        }

    }
    [Equals]
    public class ClassWithSimpleList
    {
        public List<int> someList { get; set; }

        public ClassWithSimpleList()
        {
            someList = new List<int>();
        }
    }

    [Equals]
    public class ClassWithArray
    {
        public int[] theArray { get; set; }
    }

    [Equals]
    public class ClassWithGuid
    {
        public Guid theGuid { get; set; }
    }

    [Equals]
    public class VeryComplexClass
    {
        public List<ClassWithArray> ClassesWithArray { get; set; }
        public ClassWithGuid[] ClassesWithGuid { get; set; }
        public ClassWithConstructorPopulatingProperty anotherClass { get; set; }
        public User simpleClass { get; set; }
        public int simpleObject { get; set; }
        public List<bool> bools { get; set; }
        public double doubleProperty { get; set; }
        public float floatProperty { get; set; }
    }


    [Equals]
    [DataContract(Name = "User", Namespace = "user")]
    public class AttributeClass
    {
        [DataMember(Name = "name")]
        public string StringProperty { get; set; }

        [DataMember(Name = "favorite_number")]
        [NullableSchema]
        public int? NullableIntProperty { get; set; }

        [DataMember(Name = "favorite_color")]
        public string AndAnotherString { get; set; }
    }

    [Equals]
    [DataContract(Name = "User", Namespace = "database")]
    public class SmallerAttributeClass
    {
        [DataMember(Name = "name")]
        public string StringProperty { get; set; }

        [DataMember(Name = "favorite_number")]
        [NullableSchema]
        public int? NullableIntProperty { get; set; }
    }

    [Equals]
    public class ClassWithDateTime
    {
        public string From { get; set; }
        public string To { get; set; }

        public int Count { get; set; }
        public DateTime ArriveBy { get; set; }
    }

    [Equals]
    public class ClassWithoutGetters
    {
        public string SomeString;
        public int Count;
    }
}
