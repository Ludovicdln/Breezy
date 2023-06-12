using Breezy.Generator.Tests.Models.Constraints;
using MySql.Data.MySqlClient;

namespace Breezy.Generator.Tests.Mappings;

public sealed class ManyToManyTests
{
    private MySqlConnection _mySqlConnection;

    [SetUp]
    public void Setup()
    {
        _mySqlConnection = new MySqlConnection($"server=localhost;port=3306;user id=root; password=root; database=test;");
    }
    
    [Test]
    public async Task MapManyToMany_ShouldReturnIEnumerable()
    {
        var posts = await _mySqlConnection.QueryAsync<Post>(
            @"SELECT * FROM test.post p INNER JOIN posts_tags pt ON p.id = pt.post_id INNER JOIN tag t ON t.id = pt.tag_id");
        
        Assert.Multiple(() =>
        {
            Assert.That(posts.Any(), Is.True);
            Assert.That(posts.All(x => x.IsValid()), Is.True);
            Assert.That(posts.Any(x => x.Tags.Count > 0));
            Assert.That(posts.Any(x => x.Tags.Any(y => y.Posts.Count > 0)));
        });
    }
    
    [Test]
    public async Task MapManyToMany_ShouldReturnFirstElement()
    {
        var post = await _mySqlConnection.QueryFirstOrDefaultAsync<Post>(
            @"SELECT * FROM test.post p INNER JOIN posts_tags pt ON p.id = pt.post_id INNER JOIN tag t ON t.id = pt.tag_id AND p.id = 1");
        
        Assert.Multiple(() =>
        {
            Assert.That(post, Is.Not.Null);
            Assert.That(post.Id, Is.EqualTo(1));
            Assert.That(post.IsValid, Is.True);
        });
    }
    
    [Test]
    public async Task MapManyToMany_ShouldReturnFirstElement_WithAnonymousType()
    {
        var post = await _mySqlConnection.QueryFirstOrDefaultAsync<Post>(
            @"SELECT * FROM test.post p INNER JOIN posts_tags pt ON p.id = pt.post_id INNER JOIN tag t ON t.id = pt.tag_id AND p.id = @Id", new { Id = 1});
        
        Assert.Multiple(() =>
        {
            Assert.That(post, Is.Not.Null);
            Assert.That(post.Id, Is.EqualTo(1));
            Assert.That(post.IsValid, Is.True);
        });
    }
}