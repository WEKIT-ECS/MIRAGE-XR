
# Coding Guidelines

Generally we follow the dotnet C# style and code conventions as described [microsoft dotnet documentation](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) you can use this as a reference.

However there are a few different or Unity specific conventions we use in our modules that you will find described below.

### Private class fields

Similar to the dotnet C# conventions we use [Camel case](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#camel-case) for private fields however we do not use the `_` prefix and as such we encourage you to do the same to keep the code consistent, see below for an example.

```cs
public class Avatar
{
    private string avatarName;
}

```

### Constant class fields

For constant class fields our convention is to use SCREAMING_SNAKE_CASE.


```cs
public class Avatar
{
    public const string AVATAR_NAME;
}

```