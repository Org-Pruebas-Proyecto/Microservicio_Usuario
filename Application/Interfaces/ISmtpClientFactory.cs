namespace Application.Interfaces;

public interface ISmtpClientFactory
{
    ISmtpClient Create();
}