
namespace LiBookerWebApi.Endpoints
{
    internal static class ParamChecker
    {
        /// <summary>
        /// Checks whether the provided nullable ID is invalid (less than zero).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static bool IsNotIdParamOk(ref int? id)
        {
            return id is not null && id < 0;
        }

        /// <summary>
        /// Checks whether the provided ID is invalid (less than zero).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static bool IsInvalidId(int id)
        {
            return id < 0;
        }
    }
}
