﻿using System;
using SolTechnology.Avro.Exceptions;
using SolTechnology.Avro.Schema;

namespace SolTechnology.Avro.Write.Resolvers
{
    public class Fixed
    {
        public Encoder.WriteItem Resolve(FixedSchema es)
        {
            return (value, encoder) =>
            {
                if (value is Guid guid)
                {
                    value = new Models.Fixed(es, guid.ToByteArray());
                }

                else if (!(value is Models.Fixed) || !((Models.Fixed)value).Schema.Equals(es))
                {
                    throw new AvroTypeMismatchException("[GenericFixed] required to write against [Fixed] schema but found " + value.GetType());
                }

                Models.Fixed ba = (Models.Fixed)value;
                encoder.WriteFixed(ba.Value);
            };
        }
    }
}
