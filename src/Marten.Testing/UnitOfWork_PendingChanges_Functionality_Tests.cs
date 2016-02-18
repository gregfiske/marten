﻿using System;
using System.Linq;
using Baseline;
using Marten.Testing.Documents;
using Marten.Testing.Fixtures;
using Shouldly;
using Xunit;

namespace Marten.Testing
{
    public class UnitOfWork_PendingChanges_Functionality_Tests : IntegratedFixture
    {
        [Fact]
        public void pending_changes_from_store()
        {
            using (var session = theStore.LightweightSession())
            {
                var user1 = new User();
                var user2 = new User();

                session.Store(user1, user2);

                var target1 = new Target();
                var target2 = new Target();

                session.Store(target1, target2);


                session.PendingChanges.Updates().Count().ShouldBe(4);
                session.PendingChanges.Updates().ShouldContain(user1);
                session.PendingChanges.Updates().ShouldContain(user2);
                session.PendingChanges.Updates().ShouldContain(target1);
                session.PendingChanges.Updates().ShouldContain(target2);
            }
        }

        [Fact]
        public void pending_changes_by_type()
        {
            using (var session = theStore.LightweightSession())
            {
                var user1 = new User();
                var user2 = new User();

                session.Store(user1, user2);

                var target1 = new Target();
                var target2 = new Target();

                session.Store(target1, target2);


                session.PendingChanges.UpdatesFor<User>().ShouldHaveTheSameElementsAs(user1, user2);
                session.PendingChanges.UpdatesFor<Target>().ShouldHaveTheSameElementsAs(target1, target2);
            }
        }

        [Fact]
        public void pending_deletions()
        {
            using (var session = theStore.LightweightSession())
            {
                var user1 = new User();
                var user2 = new User();

                session.Delete(user1);
                session.Delete<User>(user2.Id);

                var target1 = new Target();

                session.Delete(target1);

                session.PendingChanges.Deletions().Count().ShouldBe(3);
                session.PendingChanges.DeletionsFor(typeof(Target)).Single().Id.ShouldBe(target1.Id);
                session.PendingChanges.DeletionsFor(typeof(Target)).Single().Document.ShouldBe(target1);

                session.PendingChanges.DeletionsFor<User>().Count().ShouldBe(2);
                session.PendingChanges.DeletionsFor<User>().Any(x => x.Document == user1).ShouldBeTrue();
                session.PendingChanges.DeletionsFor<User>().Any(x => x.Id.As<Guid>() == user2.Id).ShouldBeTrue();
            }
        }

        [Fact]
        public void pending_with_dirty_checks()
        {
            var user1 = new User();
            var user2 = new User();

            using (var session1 = theStore.LightweightSession())
            {
                session1.Store(user1, user2);
                session1.SaveChanges();
            }

            using (var session2 = theStore.DirtyTrackedSession())
            {
                var user12 = session2.Load<User>(user1.Id);
                var user22 = session2.Load<User>(user2.Id);
                user12.FirstName = "Hank";

                session2.PendingChanges.UpdatesFor<User>().Single()
                    .ShouldBe(user12);

                session2.PendingChanges.UpdatesFor<User>().ShouldNotContain(user22);
            }
        }
    }
}