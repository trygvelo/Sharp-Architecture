namespace Tests.SharpArch.Domain.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using global::SharpArch.Domain.DomainModel;

    using NUnit.Framework;

    [TestFixture]
    public class BaseObjectEqualityComparerTests
    {
        [Test]
        public void CanBeUsedByLinqSetOperatorsSuchAsIntersect()
        {
            IList<ConcreteEntityWithDomainSignatureProperties> objects1 =
                new List<ConcreteEntityWithDomainSignatureProperties>();
            var object1 = new ConcreteEntityWithDomainSignatureProperties { Name = "Billy McCafferty", };
            EntityIdSetter.SetIdOf(object1, new Guid("79E9F560-FD70-4807-BEED-50A87AA911B1"));
            objects1.Add(object1);

            IList<ConcreteEntityWithDomainSignatureProperties> objects2 =
                new List<ConcreteEntityWithDomainSignatureProperties>();
            var object2 = new ConcreteEntityWithDomainSignatureProperties { Name = "Jimi Hendrix", };
            EntityIdSetter.SetIdOf(object2, new Guid("52335ABA-2D8B-4892-A8B7-86B817AAC607"));
            objects2.Add(object2);
            var object3 = new ConcreteEntityWithDomainSignatureProperties
                {
                    Name =
                        "Doesn't Matter since the Id will match and the presedence of the domain signature will go overridden", 
                };
            EntityIdSetter.SetIdOf(object3, new Guid("52335ABA-2D8B-4892-A8B7-86B817AAC607"));
            objects2.Add(object3);

            Assert.That(
                objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>()).Count(), 
                Is.EqualTo(1));
            Assert.AreEqual(
                objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>()).First(), 
                object1);
            Assert.AreEqual(
                objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>()).First(), 
                object3);
        }

        [Test]
        public void CanCompareBaseObjects()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var object1 = new ConcreteBaseObject { Name = "Whatever" };
            var object2 = new ConcreteBaseObject { Name = "Whatever" };
            Assert.That(comparer.Equals(object1, object2));

            object2.Name = "Mismatch";
            Assert.That(comparer.Equals(object1, object2), Is.False);
        }

        [Test]
        public void CanCompareEntitiesWithDomainSignatureProperties()
        {
            var comparer = new BaseObjectEqualityComparer<Entity>();

            var object1 = new ConcreteEntityWithDomainSignatureProperties { Name = "Whatever" };
            var object2 = new ConcreteEntityWithDomainSignatureProperties { Name = "Whatever" };
            Assert.That(comparer.Equals(object1, object2));

            object2.Name = "Mismatch";
            Assert.That(comparer.Equals(object1, object2), Is.False);

            EntityIdSetter.SetIdOf(object1, new Guid("79E9F560-FD70-4807-BEED-50A87AA911B1"));
            EntityIdSetter.SetIdOf(object2, new Guid("79E9F560-FD70-4807-BEED-50A87AA911B1"));
            Assert.That(comparer.Equals(object1, object2));
        }

        [Test]
        public void CanCompareEntitiesWithNoDomainSignatureProperties()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var object1 = new ConcreteEntityWithNoDomainSignatureProperties { Name = "Whatever" };
            var object2 = new ConcreteEntityWithNoDomainSignatureProperties { Name = "asdf" };
            Assert.That(comparer.Equals(object1, object2), Is.False);

            EntityIdSetter.SetIdOf(object1, new Guid("79E9F560-FD70-4807-BEED-50A87AA911B1"));
            EntityIdSetter.SetIdOf(object2, new Guid("79E9F560-FD70-4807-BEED-50A87AA911B1"));
            Assert.That(comparer.Equals(object1, object2));
        }

        [Test]
        public void CanCompareNulls()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();
            Assert.That(comparer.Equals(null, null));
            Assert.That(comparer.Equals(null, new ConcreteBaseObject()), Is.False);
            Assert.That(comparer.Equals(new ConcreteBaseObject(), null), Is.False);
        }

        [Test]
        public void CanCompareValueObjects()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var object1 = new ConcreteValueObject { Name = "Whatever" };
            var object2 = new ConcreteValueObject { Name = "Whatever" };
            Assert.That(comparer.Equals(object1, object2));

            object2.Name = "Mismatch";
            Assert.That(comparer.Equals(object1, object2), Is.False);
        }

        [Test]
        public void CannotSuccessfullyCompareDifferentlyTypedObjectsThatDeriveFromBaseObject()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var object1 = new ConcreteBaseObject { Name = "Whatever" };
            var object2 = new ConcreteValueObject { Name = "Whatever" };

            Assert.That(comparer.Equals(object1, object2), Is.False);
        }

        private class ConcreteBaseObject : BaseObject
        {
            public string Name { get; set; }

            protected override IEnumerable<PropertyInfo> GetTypeSpecificSignatureProperties()
            {
                return this.GetType().GetProperties();
            }
        }

        private class ConcreteEntityWithDomainSignatureProperties : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        private class ConcreteEntityWithNoDomainSignatureProperties : Entity
        {
            public string Name { get; set; }
        }

        private class ConcreteValueObject : ValueObject
        {
            public string Name { get; set; }
        }
    }
}