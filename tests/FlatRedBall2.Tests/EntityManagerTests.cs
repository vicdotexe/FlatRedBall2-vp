using Shouldly;
using Xunit;

namespace FlatRedBall2.Tests;

public class EntityManagerTests
{
    [Fact]
    public void Clear_WithRegisteredEntities_EmptiesList()
    {
        var manager = new EntityManager();
        manager.Register(new Entity());
        manager.Register(new Entity());

        manager.Clear();

        manager.Entities.Count.ShouldBe(0);
    }

    [Fact]
    public void Register_AddsEntityToEntitiesList()
    {
        var manager = new EntityManager();
        var entity = new Entity();

        manager.Register(entity);

        manager.Entities.ShouldContain(entity);
    }

    [Fact]
    public void Unregister_RemovesEntityFromEntitiesList()
    {
        var manager = new EntityManager();
        var entity = new Entity();
        manager.Register(entity);

        manager.Unregister(entity);

        manager.Entities.ShouldNotContain(entity);
    }
}
