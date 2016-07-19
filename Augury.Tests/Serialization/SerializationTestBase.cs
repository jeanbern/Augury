using Augury.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Augury.Test.Serialization
{
    [TestClass]
    public abstract class SerializationTestBase<T>
    {
        protected abstract T CreateInstance();

        protected virtual ISerializer<T> CreateSerializer()
        {
            var classType = typeof(T);
            var genericInterface = typeof(ISerializer<>);
            var serializerInterface = genericInterface.MakeGenericType(classType);
            var serializerType = Base.Serialization.GetSerializer(serializerInterface);

            return (ISerializer<T>) Activator.CreateInstance(serializerType);
        }

        protected virtual bool Equals(T x, T y)
        {
            return x.Equals(y);
        }

        [TestMethod]
        public void SerializationEqualityTest()
        {
            var serializer = CreateSerializer();
            var instance = CreateInstance();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, instance);
                stream.Seek(0, SeekOrigin.Begin);
                var instance2 = serializer.Deserialize(stream);
                //So the line above doesn't get optimized away. I don't think that can happen though.
                GC.KeepAlive(instance2);
                Assert.IsTrue(Equals(instance, instance2));
            }
        }
    }
}
