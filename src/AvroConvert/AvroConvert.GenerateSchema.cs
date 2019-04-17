﻿namespace Avro
{
    using Microsoft.Hadoop.Avro;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    public static partial class AvroConvert
    {
        public static string GenerateSchema(object obj)
        {
            object inMemoryInstance = DecorateObjectWithAvroAttributes(obj);

            //invoke Create method of AvroSerializer
            var createMethod = typeof(AvroSerializer).GetMethod("Create", new Type[0]);
            var createGenericMethod = createMethod.MakeGenericMethod(inMemoryInstance.GetType());
            dynamic avroSerializer = createGenericMethod.Invoke(inMemoryInstance, null);

            string result = avroSerializer.GetType().GetProperty("WriterSchema").GetValue(avroSerializer, null).ToString();

            return result;
        }

        private static object DecorateObjectWithAvroAttributes(object obj)
        {
            //generate in memory assembly with mimic type
            Type objType = obj.GetType();
            var assemblyName = new System.Reflection.AssemblyName("InMemory");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            TypeBuilder typeBuilder;



            typeBuilder = moduleBuilder.DefineType(objType.Name, System.Reflection.TypeAttributes.Public);

            if (typeof(IList).IsAssignableFrom(objType))
            {

            }
            else
            {
                
            
                PropertyInfo[] properties = objType.GetProperties();
                foreach (var prop in properties)
                {
                    Type properType = prop.PropertyType;

                    if (typeof(IList).IsAssignableFrom(properType))
                    {
                        // We have a List<T> or array
                        PropertyInfo[] fields = properType.GetProperties();
                        Type listedType = fields[2].PropertyType;


                        var listedObject = Activator.CreateInstance(listedType);
                        var listedObject2 = DecorateObjectWithAvroAttributes(listedObject);
                        var tempArray = Array.CreateInstance(listedObject2.GetType(), 1);

                        var tempArray2 = DecorateObjectWithAvroAttributes(tempArray);
                        //   var tempArray = Array.CreateInstance(listedType, 1);
                        var newType = DecorateObjectWithAvroAttributes(tempArray.GetType()).GetType();
                        //  typeBuilder = moduleBuilder.DefineType(objType.Name, System.Reflection.TypeAttributes.Public);

                        // typeBuilder = AddPropertyToTypeBuilderInCaseOfList(typeBuilder, newType, prop.Name, null);

                        typeBuilder = AddPropertyToTypeBuilder(typeBuilder, newType, prop.Name, null);

                    }
                    else
                    {
                        typeBuilder = AddPropertyToTypeBuilder(typeBuilder, properType, prop.Name, prop.GetValue(obj));
                    }
                }



            }
            var dataContractAttributeConstructor = typeof(DataContractAttribute).GetConstructor(new Type[] { });
            var dataContractAttributeProperties = typeof(DataContractAttribute).GetProperties();
            CustomAttributeBuilder dataContractAttributeBuilder = new CustomAttributeBuilder(dataContractAttributeConstructor, new string[] { }, dataContractAttributeProperties.Where(p => p.Name == "Name").ToArray(), new object[] { objType.Name });

            typeBuilder.SetCustomAttribute(dataContractAttributeBuilder);


            var inMemoryType = typeBuilder.CreateType();
            var inMemoryInstance = Activator.CreateInstance(inMemoryType);

            return inMemoryInstance;
        }

        private static object DecorateObjectWithAvroAttributes(Type objType)
        {
            //generate in memory assembly with mimic type
            var assemblyName = new System.Reflection.AssemblyName("InMemory");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            TypeBuilder typeBuilder;


            typeBuilder = moduleBuilder.DefineType(objType.Name, System.Reflection.TypeAttributes.Public);

            PropertyInfo[] properties = objType.GetProperties();
            foreach (var prop in properties)
            {
                Type properType = prop.PropertyType;

                if (typeof(IList).IsAssignableFrom(properType))
                {
                    // We have a List<T> or array
                    PropertyInfo[] fields = properType.GetProperties();
                    Type listedType = fields[2].PropertyType;


                    var listedObject = Activator.CreateInstance(listedType);
                    //       var listedObject2 = DecorateObjectWithAvroAttributes(listedObject);
                    var tempArray = Array.CreateInstance(listedObject.GetType(), 1);
                    //   var tempArray = Array.CreateInstance(listedType, 1);
                    var newType = tempArray.GetType();
                    //  typeBuilder = moduleBuilder.DefineType(objType.Name, System.Reflection.TypeAttributes.Public);

                    typeBuilder = AddPropertyToTypeBuilderInCaseOfList(typeBuilder, newType, prop.Name, null);
                }
                else
                {
                    typeBuilder = AddPropertyToTypeBuilder(typeBuilder, properType, prop.Name, null);
                }

            }

            var dataContractAttributeBuilder = GenerateCustomAttributeBuilder<DataContractAttribute>(objType.Name);

            typeBuilder.SetCustomAttribute(dataContractAttributeBuilder);

            var inMemoryType = typeBuilder.CreateType();
            var inMemoryInstance = Activator.CreateInstance(inMemoryType);

            return inMemoryInstance;
        }

        public static CustomAttributeBuilder GenerateCustomAttributeBuilder<T>(string name)
        {
            var attributeConstructor = typeof(T).GetConstructor(new Type[] { });
            var attributeProperties = typeof(T).GetProperties();
            var attributeBuilder = new CustomAttributeBuilder(attributeConstructor, new string[] { }, attributeProperties.Where(p => p.Name == "Name").ToArray(), new object[] { name });

            return attributeBuilder;
        }

        private static TypeBuilder AddPropertyToTypeBuilder(TypeBuilder typeBuilder, Type properType, string name, object value = null)
        {
            //if complex type - use recurrence
            if (!(properType.GetTypeInfo().IsValueType ||
             properType == typeof(string) || value == null))
            {
                properType = DecorateObjectWithAvroAttributes(value).GetType();
            }

//            else if (typeof(IList).IsAssignableFrom(properType))
//            {
//                properType = DecorateObjectWithAvroAttributes(properType).GetType();
//            }

            //mimic property 
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, properType, null);

            var attributeConstructor = typeof(DataMemberAttribute).GetConstructor(new Type[] { });
            var attributeProperties = typeof(DataMemberAttribute).GetProperties();
            var attributeBuilder = new CustomAttributeBuilder(attributeConstructor, new string[] { }, attributeProperties.Where(p => p.Name == "Name").ToArray(), new object[] { name });

            propertyBuilder.SetCustomAttribute(attributeBuilder);

            var dataContractAttributeBuilder = GenerateCustomAttributeBuilder<DataContractAttribute>(name);

            propertyBuilder.SetCustomAttribute(dataContractAttributeBuilder);

            //Is nullable 
            if (Nullable.GetUnderlyingType(properType) != null)
            {
                var nullableAttributeConstructor = typeof(NullableSchemaAttribute).GetConstructor(new Type[] { });
                var nullableAttributeBuilder = new CustomAttributeBuilder(nullableAttributeConstructor, new string[] { }, new PropertyInfo[] { }, new object[] { });

                propertyBuilder.SetCustomAttribute(nullableAttributeBuilder);
            }

            // Define field
            FieldBuilder fieldBuilder = typeBuilder.DefineField(name, properType, FieldAttributes.Public);

            // Define "getter" for MyChild property
            MethodBuilder getterBuilder = typeBuilder.DefineMethod("get_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                properType,
                Type.EmptyTypes);
            ILGenerator getterIL = getterBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // Define "setter" for MyChild property
            MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { properType });
            ILGenerator setterIL = setterBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
            return typeBuilder;
        }

        private static TypeBuilder AddPropertyToTypeBuilderInCaseOfList(TypeBuilder typeBuilder, Type properType, string name, object value = null)
        {
           
           //     properType = DecorateObjectWithAvroAttributes(properType).GetType();
            

            //mimic property 
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, properType, null);

            var attributeConstructor = typeof(DataMemberAttribute).GetConstructor(new Type[] { });
            var attributeProperties = typeof(DataMemberAttribute).GetProperties();
            var attributeBuilder = new CustomAttributeBuilder(attributeConstructor, new string[] { }, attributeProperties.Where(p => p.Name == "Name").ToArray(), new object[] { name });

         //   propertyBuilder.SetCustomAttribute(attributeBuilder);

            var lol = GenerateCustomAttributeBuilder<DataContractAttribute>(name);
        //    propertyBuilder.SetCustomAttribute(lol);

            //Is nullable 
            if (Nullable.GetUnderlyingType(properType) != null)
            {
                var nullableAttributeConstructor = typeof(NullableSchemaAttribute).GetConstructor(new Type[] { });
                var nullableAttributeBuilder = new CustomAttributeBuilder(nullableAttributeConstructor, new string[] { }, new PropertyInfo[] { }, new object[] { });

                propertyBuilder.SetCustomAttribute(nullableAttributeBuilder);
            }

            // Define field
            FieldBuilder fieldBuilder = typeBuilder.DefineField(name, properType, FieldAttributes.Public);

            // Define "getter" for MyChild property
            MethodBuilder getterBuilder = typeBuilder.DefineMethod("get_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                properType,
                Type.EmptyTypes);
            ILGenerator getterIL = getterBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // Define "setter" for MyChild property
            MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { properType });
            ILGenerator setterIL = setterBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);

            return typeBuilder;
        }
    }
}