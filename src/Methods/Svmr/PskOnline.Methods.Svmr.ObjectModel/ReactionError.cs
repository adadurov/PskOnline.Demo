namespace PskOnline.Methods.Svmr.ObjectModel
{
  public enum ReactionError
  {
    // Нормальная реакция
    NoError,

    // Преждевременная реакция
    Premature,

    // Пропущенная реакция (кнопка не нажата за максимально допустимое время)
    Missed,

    // Прерванный атом
    Cancelled,

    // Логическая ошибка теста
    LogicError
  }
}
