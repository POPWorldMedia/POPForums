namespace PopForums.Services.Interfaces;

public interface ITimeFormatStringService
{
    TimeFormats GeTimeFormats();

    string GetTimeFormatsAsJson();
}
