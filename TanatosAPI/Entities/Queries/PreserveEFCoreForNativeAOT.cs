using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Queries {
    public static class PreserveEFCoreForNativeAOT {

        public static void Ensure() {
            _ = typeof(RuntimeModel);
            _ = typeof(ShapedQueryCompilingExpressionVisitorDependencies);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RuntimeModel))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ShapedQueryCompilingExpressionVisitorDependencies))]
        private static void Preserve() { }
    }
}
