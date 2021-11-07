using System.Collections.Generic;
namespace DynamicRuntime {
	public interface ICallable {
		object Call(IReadOnlyList<object> args);
	}
}
