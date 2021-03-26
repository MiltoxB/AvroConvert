#region license
/**Copyright (c) 2021 Adrian Struga�a
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* https://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using SolTechnology.Avro.Extensions;
using SolTechnology.Avro.Schema.Abstract;

namespace SolTechnology.Avro.Schema
{

    internal sealed class TimestampMillisecondsSchema : LogicalTypeSchema
    {
        public TimestampMillisecondsSchema() : this(typeof(DateTime))
        {
        }
        public TimestampMillisecondsSchema(Type runtimeType) : base(runtimeType)
        {
            BaseTypeSchema = new LongSchema();
        }

        internal override AvroType Type => Avro.Schema.AvroType.Logical;
        internal override TypeSchema BaseTypeSchema { get; set; }
        internal override string LogicalTypeName => "timestamp-millis";

        internal object ConvertToBaseValue(object logicalValue, TimestampMillisecondsSchema schema)
        {
            DateTime date;
            if (logicalValue is DateTimeOffset dateTimeOffset)
            {
                date = dateTimeOffset.DateTime;
            }
            else
            {
                date = ((DateTime)logicalValue);
            }

            return (long)(date - DateTimeExtensions.UnixEpochDateTime).TotalMilliseconds;
        }

        internal override object ConvertToLogicalValue(object baseValue, LogicalTypeSchema schema, Type type)
        {
            var noMs = (long)baseValue;
            var result =  DateTimeExtensions.UnixEpochDateTime.AddMilliseconds(noMs);

            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(noMs);
            }
            else
            {
                return result;
            }
        }
    }
}
