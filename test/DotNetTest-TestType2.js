const edge = require('edge');
const EdgeReference = require('edge-reference');

var Constructor = edge.func({ source: () => {/*
#r "./node_modules/edge-reference/bin/EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> ConstructInstance(dynamic input)
    {
        DotNetTest.TestType2 _result = new DotNetTest.TestType2();
        return ReferenceManager.Instance.EnsureReference(_result);
    }
}
*/
}, 
    methodName: 'ConstructInstance'
});

var Get_Description = edge.func({ source: () => {/*
#r "./node_modules/edge-reference/bin/EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_Description(object _referenceId)
    {
        long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;

        DotNetTest.TestType2 _parent = (DotNetTest.TestType2)ReferenceManager.Instance.PullReference(_refId);
        System.String _result = _parent.Description;
        return _result;
    }
}
*/
}, 
    methodName: 'Get_Description'
});

var Set_Description = edge.func({ source: () => {/*
#r "./node_modules/edge-reference/bin/EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task Set_Description(dynamic parameters)
    {
        long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

        DotNetTest.TestType2 _parent = (DotNetTest.TestType2)ReferenceManager.Instance.PullReference(_refId);
        _parent.Description = parameters.value;
    }
}
*/
}, 
    methodName: 'Set_Description'
});

var Get_Status = edge.func({ source: () => {/*
#r "./node_modules/edge-reference/bin/EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_Status(object _referenceId)
    {
        long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;

        DotNetTest.TestType2 _parent = (DotNetTest.TestType2)ReferenceManager.Instance.PullReference(_refId);
        System.Int32 _result = _parent.Status;
        return _result;
    }
}
*/
}, 
    methodName: 'Get_Status'
});

var Set_Status = edge.func({ source: () => {/*
#r "./node_modules/edge-reference/bin/EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task Set_Status(dynamic parameters)
    {
        long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

        DotNetTest.TestType2 _parent = (DotNetTest.TestType2)ReferenceManager.Instance.PullReference(_refId);
        _parent.Status = parameters.value;
    }
}
*/
}, 
    methodName: 'Set_Status'
});

class TestType2 extends EdgeReference {

    constructor(referenceId, args) {
        super(referenceId, Constructor, args);
    }


    get Description() {
        return Get_Description(this._referenceId, true);
    }

    set Description(value) {
        Set_Description({ _referenceId: this._referenceId, value: value }, true);
    }

    get Status() {
        return Get_Status(this._referenceId, true);
    }

    set Status(value) {
        Set_Status({ _referenceId: this._referenceId, value: value }, true);
    }

}

module.exports = TestType2
