namespace MiniLedger.Application.Abstractions;

public interface ICommand<TResponse> : IRequest<TResponse>;
public interface IQuery<TResponse>   : IRequest<TResponse>;