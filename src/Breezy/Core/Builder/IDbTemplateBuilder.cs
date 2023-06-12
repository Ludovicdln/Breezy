namespace Breezy.Core.Builder;

public interface IDbTemplateBuilder
{
    public IDbTemplateBuilder GenerateQueryAsyncFunction();
    public IDbTemplateBuilder GenerateQueryFirstAsyncFunction();
    public IDbTemplateBuilder GenerateExecuteAsyncFunction();
    public string Build(CancellationToken cancellationToken = default);
}