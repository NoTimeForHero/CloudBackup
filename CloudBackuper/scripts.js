/*
    Доступные глобальные переменные внутри модуля:
    DateTime datetime - Дата и время выполнения задачи как JS object (у которого не работает C# метод ToString(format))
    string guid - Случайный GUID для имени файла
    string md5(string input) - MD5 от заданной строки
    string no_space(string input, string target="_") - заменить все пробелы в имени на указанный во 2 аргументе символ (подстроку)
    string format_datetime(string format, string culture_info=null) - форматировать текущую дату-время по строке формата
    void debug(string message) - отправить сообщение в логгер с уровнем DEBUG
*/
// ReSharper disable UseOfImplicitGlobalInFunctionScope

/**
 * Функция, генерирующая имя файла для S3 хранилища, под которым будет записан архив.
 * @param {string} taskName Название задачи резервного копирование
 * @return {string} Имя файла или путь (содержащий /), под которым оно будет загружено в S3 облака (без расширения ZIP!)
 */
function getCloudFilename(taskName) {
    return taskName;
    // Пример для возврата "День недели/Имя_задачи"
    /*
    var path = "";
    path += format_datetime("dddd", "ru_RU").toLowerCase();
    path += "/";
    path += no_space(taskName);
    return path;
    */
}