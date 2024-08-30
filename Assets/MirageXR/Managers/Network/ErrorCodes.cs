namespace MirageXR.NewDataModel
{
    public enum ErrorCodes
    {
        // Defaults
        None = 0, // Status code: 400
        NullValue, // Status code: 500
        NotFound, // Status code: 404
        Conflict, // Status code: 409
        Failure, // Status code: 500
        Validation, // Status code: 400
        InternalServerError, // Status code: 500
        Forbidden, // Status code: 403
    }
}