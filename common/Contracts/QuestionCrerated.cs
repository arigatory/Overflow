namespace Contracts;

public record QuestionCrerated(
    string QuestionId,
    string Title,
    string Content,
    DateTime Created,
    List<string> Tags
);