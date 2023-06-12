namespace Breezy.Core.IO.Converter;

public interface IConverter<in TInput, out TOutput> 
    where TInput : IConverted 
    where TOutput : IConverted
{
    public TOutput Convert(TInput input, object? param = null);
    public IEnumerable<TOutput> Convert(IEnumerable<TInput> inputs, object? param = null);
}