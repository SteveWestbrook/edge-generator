const edge = require('edge');
const EdgeReference = require('edge-reference');
const TestType2 = require('./DotNetTest-TestType2.js');

var Constructor = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> ConstructInstance(dynamic input)
    {
        DotNetTest.TestType1 _result = new DotNetTest.TestType1();
        return ReferenceManager.Instance.EnsureReference(_result);
    }
}
*/
}, 
    methodName: 'ConstructInstance'
});

var Get_SharedData = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_SharedData(object unused)
    {
        System.String _result = DotNetTest.TestType1.SharedData;
        return _result;
    }
}
*/
}, 
    methodName: 'Get_SharedData'
});

var Set_SharedData = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task Set_SharedData(dynamic parameters)

    {
        DotNetTest.TestType1.SharedData = parameters.value;
    }
}
*/
}, 
    methodName: 'Set_SharedData'
});

var Get_Child = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_Child(object _referenceId)
    {
        long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;

        DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
        DotNetTest.TestType2 _result = _parent.Child;
        return ReferenceManager.Instance.EnsureReference(_result);
    }
}
*/
}, 
    methodName: 'Get_Child'
});

var Get_Name = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_Name(object _referenceId)
    {
        long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;

        DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
        System.String _result = _parent.Name;
        return _result;
    }
}
*/
}, 
    methodName: 'Get_Name'
});

var Set_Name = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task Set_Name(dynamic parameters)
    {
        long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

        DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
        _parent.Name = parameters.value;
    }
}
*/
}, 
    methodName: 'Set_Name'
});

var Get_Sibling = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> Get_Sibling(object _referenceId)
    {
        long _refId = _referenceId is long ? (long)_referenceId : (long)(int)_referenceId;

        DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
        DotNetTest.TestType1 _result = _parent.Sibling;
        return ReferenceManager.Instance.EnsureReference(_result);
    }
}
*/
}, 
    methodName: 'Get_Sibling'
});

var Set_Sibling = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task Set_Sibling(dynamic parameters)

    {
        long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

        DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
        parameters.value = ReferenceManager.Instance.PullReference(parameters.value);

        _parent.Sibling = parameters.value;
    }
}
*/
}, 
    methodName: 'Set_Sibling'
});

var CreateT2Static = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> CreateT2Static(dynamic parameters)
    {
        return await Task<object>.Factory.StartNew(() => {
            DotNetTest.TestType2 _result = DotNetTest.TestType1.CreateT2Static();
            return ReferenceManager.Instance.EnsureReference(_result);
        });
    }
}
*/
}, 
    methodName: 'CreateT2Static'
});

var CreateT2StaticTemplate = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> CreateT2StaticTemplate(dynamic parameters)
    {
        return await Task<object>.Factory.StartNew(() => {
            parameters.template = ReferenceManager.Instance.PullReference(parameters.template);

            DotNetTest.TestType2 _result = DotNetTest.TestType1.CreateT2StaticTemplate(parameters.template);
            return ReferenceManager.Instance.EnsureReference(_result);
        });
    }
}
*/
}, 
    methodName: 'CreateT2StaticTemplate'
});

var IncreaseCount = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task IncreaseCount(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            DotNetTest.TestType1.IncreaseCount();
        });
    }
}
*/
}, 
    methodName: 'IncreaseCount'
});

var UpdateName = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task UpdateName(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            parameters.target = ReferenceManager.Instance.PullReference(parameters.target);

            DotNetTest.TestType1.UpdateName(parameters.target, parameters.name);
        });
    }
}
*/
}, 
    methodName: 'UpdateName'
});

var AssignChild = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task AssignChild(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            parameters.newValue = ReferenceManager.Instance.PullReference(parameters.newValue);

            _parent.AssignChild(parameters.newValue);
        });
    }
}
*/
}, 
    methodName: 'AssignChild'
});

var AssignName = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task AssignName(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            _parent.AssignName(parameters.name);
        });
    }
}
*/
}, 
    methodName: 'AssignName'
});

var AssignT2Description = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task AssignT2Description(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            parameters.target = ReferenceManager.Instance.PullReference(parameters.target);

            _parent.AssignT2Description(parameters.target, parameters.description);
        });
    }
}
*/
}, 
    methodName: 'AssignT2Description'
});

var CreateNewT2 = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> CreateNewT2(dynamic parameters)
    {
        return await Task<object>.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            parameters.template = ReferenceManager.Instance.PullReference(parameters.template);

            DotNetTest.TestType2 _result = _parent.CreateNewT2(parameters.template, parameters.description);
            return ReferenceManager.Instance.EnsureReference(_result);
        });
    }
}
*/
}, 
    methodName: 'CreateNewT2'
});

var DuplicateName = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"
#r "mscorlib.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task DuplicateName(dynamic parameters)
    {
        await Task.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            _parent.DuplicateName();
        });
    }
}
*/
}, 
    methodName: 'DuplicateName'
});

var ReturnSelf = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> ReturnSelf(dynamic parameters)
    {
        return await Task<object>.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            DotNetTest.TestType1 _result = _parent.ReturnSelf();
            return ReferenceManager.Instance.EnsureReference(_result);
        });
    }
}
*/
}, 
    methodName: 'ReturnSelf'
});

var ReturnTarget = edge.func({ source: () => {/*
#r "EdgeReference.dll"
#r "DotNetTest.dll"

using System.Threading.Tasks;
using EdgeReference;

public class Startup
{
    public async Task<object> ReturnTarget(dynamic parameters)
    {
        return await Task<object>.Factory.StartNew(() => {
            long _refId = parameters._referenceId is long ? (long)parameters._referenceId : (long)(int)parameters._referenceId;

            DotNetTest.TestType1 _parent = (DotNetTest.TestType1)ReferenceManager.Instance.PullReference(_refId);
            parameters.target = ReferenceManager.Instance.PullReference(parameters.target);

            DotNetTest.TestType2 _result = _parent.ReturnTarget(parameters.target);
            return ReferenceManager.Instance.EnsureReference(_result);
        });
    }
}
*/
}, 
    methodName: 'ReturnTarget'
});

class TestType1 extends EdgeReference {

    constructor(referenceId, args) {
        super(referenceId, Constructor, args);
    }


    static get SharedData() {
        return Get_SharedData(null, true);
    }

    static set SharedData(value) {
        Set_SharedData({ value: value }, true);
    }

    get Child() {
        var returnId = Get_Child(this._referenceId, true);
        return (returnId ? new TestType2(returnId) : null);
    }

    get Name() {
        return Get_Name(this._referenceId, true);
    }

    set Name(value) {
        Set_Name({ _referenceId: this._referenceId, value: value }, true);
    }

    get Sibling() {
        var returnId = Get_Sibling(this._referenceId, true);
        return (returnId ? new TestType1(returnId) : null);
    }

    set Sibling(value) {
        Set_Sibling({ _referenceId: this._referenceId, value: value._referenceId }, true);
    }

    static CreateT2Static(callback) {
        return EdgeReference.callbackOrReturn(
            CreateT2Static,
            {
                
            },
            TestType2,
            callback);
    }

    static CreateT2StaticTemplate(template, callback) {
        template = template ? template._referenceId : 0;

        return EdgeReference.callbackOrReturn(
            CreateT2StaticTemplate,
            {
                template: template
            },
            TestType2,
            callback);
    }

    static IncreaseCount(callback) {
        EdgeReference.callbackOrReturn(
            IncreaseCount,
            {
                
            },
            null,
            callback);
    }

    static UpdateName(target, name, callback) {
        target = target ? target._referenceId : 0;

        EdgeReference.callbackOrReturn(
            UpdateName,
            {
                target: target,
                name: name
            },
            null,
            callback);
    }

    AssignChild(newValue, callback) {
        newValue = newValue ? newValue._referenceId : 0;

        EdgeReference.callbackOrReturn(
            AssignChild,
            {
                _referenceId: this._referenceId,
                newValue: newValue
            },
            null,
            callback);
    }

    AssignName(name, callback) {
        EdgeReference.callbackOrReturn(
            AssignName,
            {
                _referenceId: this._referenceId,
                name: name
            },
            null,
            callback);
    }

    AssignT2Description(target, description, callback) {
        target = target ? target._referenceId : 0;

        EdgeReference.callbackOrReturn(
            AssignT2Description,
            {
                _referenceId: this._referenceId,
                target: target,
                description: description
            },
            null,
            callback);
    }

    CreateNewT2(template, description, callback) {
        template = template ? template._referenceId : 0;

        return EdgeReference.callbackOrReturn(
            CreateNewT2,
            {
                _referenceId: this._referenceId,
                template: template,
                description: description
            },
            TestType2,
            callback);
    }

    DuplicateName(callback) {
        EdgeReference.callbackOrReturn(
            DuplicateName,
            {
                _referenceId: this._referenceId
            },
            null,
            callback);
    }

    ReturnSelf(callback) {
        return EdgeReference.callbackOrReturn(
            ReturnSelf,
            {
                _referenceId: this._referenceId
            },
            TestType1,
            callback);
    }

    ReturnTarget(target, callback) {
        target = target ? target._referenceId : 0;

        return EdgeReference.callbackOrReturn(
            ReturnTarget,
            {
                _referenceId: this._referenceId,
                target: target
            },
            TestType2,
            callback);
    }

}

module.exports = TestType1
