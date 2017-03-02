const TestType1 = require('./DotNetTest-TestType1.js');
const TestType2 = require('./DotNetTest-TestType2.js');
const assert = require('assert');
const process = require('process');
const edge = require('edge');

var ReferenceCount = edge.func({ source: () => {/*
  #r "./dotnet/bin/Debug/EdgeReference.dll"

  using System.Threading.Tasks;

  public class Startup {
    public async Task<object> Invoke(object input) {
      return EdgeReference.ReferenceManager.Instance.Count;
    }
  }
*/}});

describe('TestType1', () => {
  describe('#constructor', () => {
    // TODO: Test for values above int32.maxvalue.  There is an untested conversion for long values.

    it('can be constructed', () => {
      var tt1 = new TestType1();
      assert.ok(tt1);
      assert.ok(tt1._referenceId);
    });

    it('can have a value', () => {
      var tt = new TestType1();
      tt.Name = 'asdf';
      assert.equal(tt.Name, 'asdf');
    });

    it('can transfer its reference to another instance', () => {
      var tt1 = new TestType1();
      tt1.Name = 'qwerty';
      assert.ok(tt1._referenceId);
      var tt2 = new TestType1(tt1._referenceId);
      assert.equal(tt2.Name, 'qwerty');
    });
  });

  describe('#SharedData', () => {
    it('can be read if uninitialized', () => {
      var result = TestType1.SharedData;
      assert.ok(!result);
    });

    it('can be assigned', () => {
      TestType1.SharedData = 'IAMSHARED';      
    });

    it('can be assigned and read', () => {
      var sd = 'IAMSHARED1';
      TestType1.SharedData = sd;
      assert.equal(TestType1.SharedData, sd);
    });

    it('can be assigned multiple times', () => {
      var sd = 'IAMSHARED1';
      var sd2 = 'IAMSHARED2';
      TestType1.SharedData = sd;
      assert.equal(TestType1.SharedData, sd);
      TestType1.SharedData = sd2;
      assert.equal(TestType1.SharedData, sd2);
    });
  });

  describe('#Child', () => {
    it('can be read', () => {
      var tt1 = new TestType1();
      assert.equal(tt1.Child, null);
    });

    it('cannot be assigned', () => {
      var tt1 = new TestType1();

      try {
        tt1.Child = new TestType2();
        assert.fail();
      } catch (e) {
        assert.ok(1);
      }
    });

    it('can be modified by AssignChild', (done) => {
      var tt1 = new TestType1();
      assert.equal(tt1.Child, null);
      var tt2 = new TestType2();
      tt1.AssignChild(tt2, (err, result) => {
        if (err) {
          console.dir(err);
          assert.fail();
        }
        
        assert.equal(tt1.Child._referenceId, tt2._referenceId);
        done();
      });
    });
  });

  describe('#Sibling', () => {
    it('can be read uninitialized and return null', () => {
      var main = new TestType1();
      assert.equal(main.Sibling, null);
    });

    it('can be assigned', () => {
      var main = new TestType1();
      var sib = new TestType1();
      
      main.Sibling = sib;
      assert.equal(main.Sibling._referenceId, sib._referenceId);
    });

    it('is not shared by other instances', () => {
      var main = new TestType1();
      var sib = new TestType1();

      main.Sibling = sib;
      assert.equal(sib.Sibling, null);
    });
  });

  describe('#CreateT2Static', () => {

    it(
      'creates a valid TestType2 object with a non-zero referenceID',
      (done) => {
        TestType1.CreateT2Static((err, result) => {
          if (err) {
            assert.fail(err, 'T2');
            done();
          }

          assert.ok(result);
          assert.notEqual(result._referenceId, 0);
          done();
        });
      });
  });

  describe('#CreateT2StaticTemplate', () => {
    
    it(
      'creates a TestType2 with a status matching that of its template',
      (done) => {
        var tt2Template = new TestType2();
        tt2Template.Status = 7;
        TestType1.CreateT2StaticTemplate(tt2Template, (err, result) => {
          if (err) {
            assert.fail(err, 'valid result')
            done();
          }

          assert.ok(result);
          assert.ok(result.Status);
          assert.equal(result.Status, 7);
          assert.equal(result.Status, tt2Template.Status);
          
          done();
        });
      });
  });

  describe('#IncreaseCount', () => {
    
    it('Can be called without raising an error', (done) => {
      TestType1.IncreaseCount((err, result) => {
        assert.ok(!err);

        TestType1.IncreaseCount((err2, result2) => {
          assert.ok(!err2);
          done();
        });
      });
    });
  });

  describe('#UpdateName', () => {
    it('can successfully update a name', (done) => {
      var tt1 = new TestType1();
      TestType1.UpdateName(tt1, 'newname', (err, result) => {
        assert.ok(!err);
        assert.equal(tt1.Name, 'newname');
        done();
      });
    });

    it('will provide an error when no valid entry is provided', (done) => {
      TestType1.UpdateName(null, 'newname', (err, result) => {
        assert.ok(err);
        done();
      });
    });
  });

  describe('#AssignChild', () => {
    it('can assign a valid child value', (done) => {
      var tt1 = new TestType1();
      var tt2 = new TestType2();
      tt2.Description = 'child member';
      tt1.AssignChild(tt2, (err, result) => {
        assert.ok(tt1.Child);
        assert.equal(tt1.Child._referenceId, tt2._referenceId);
        assert.equal(tt1.Child.Description, 'child member');
        done();
      });
    });

    it('can assign a null value to the child', (done) => {
      var tt1 = new TestType1();
      var tt2 = new TestType2();
      tt2.Description = 'child member';
      tt1.AssignChild(tt2, (err, result) => {
        assert.ok(tt1.Child); // make sure it's assigned

        tt1.AssignChild(null, (err2, result2) => {
          assert.ok(!err2);
          assert.ok(!tt1.Child); // make sure assignment is cleared.
          done();
        });
      });
    });
  });

  describe('#AssignName', () => {
    it('can receive a valid name', (done) => {
      var tt1 = new TestType1();
      var name = 'this is the new name';
      tt1.AssignName(name, (err, result) => {
        assert.ok(!err);
        assert.equal(tt1.Name, name);
        done();
      });
    });

    it('can clear the name given', (done) => {
      var tt1 = new TestType1();
      var name = 'toclear';
      tt1.AssignName(name, (err, result) => {
        assert.ok(!err);
        assert.equal(tt1.Name, name);
        tt1.AssignName(null, (err2, result2) => {
          assert.ok(!err2);
          assert.ok(!tt1.Name);
          done();
        });
      })
    });
  });

  describe('#AssignT2Description', () => {
    it('can assign a description to a t2 instance', (done) => {
      var tt1 = new TestType1();
      var tt2 = new TestType2();

      tt1.AssignT2Description(tt2, 'desc', (err, result) => {
        assert.ok(!err);
        assert.equal(tt2.Description, 'desc');
        done();
      });
    });

    it('will fail with a null instance', (done) => {
      var tt1 = new TestType1();
      tt1.AssignT2Description(null, 'desc', (err, result) => {
        assert.ok(err);
        done();
      });
    });

    it('will fail nicely if the wrong type is supplied', (done) => {
      var tt1 = new TestType1();
      var ttwrong = new TestType1();

      tt1.AssignT2Description(ttwrong, 'desc', (err, result) => {
        assert.ok(err);
        done();
      });
    });
  });

  describe('#CreateNewT2', () => {
    it(
      'can create a new T2 instance based on the supplied template',
      (done) => {
        var tt1 = new TestType1();
        var template = new TestType2();
        template.Status = 99;
        var desc = 'newdesc';

        tt1.CreateNewT2(template, desc, (err, result) => {
          assert.ok(!err);
          assert.ok(result);
          assert.ok(result.Status);
          assert.equal(result.Status, 99);
          assert.ok(result.Description);
          assert.equal(result.Description, desc);
          done();
        });
      });
  });

  describe('#DuplicateName', () => {
    it('can duplicate the name property', (done) => {
      var tt1 = new TestType1();
      tt1.Name = '123';
      tt1.DuplicateName((err, result) => {
        assert.ok(!err);
        assert.equal(tt1.Name, '123123');
        done();
      });
    });
  });

  describe('#ReturnSelf', () => {
    it('can return itself asynchronously', (done) => {
      var tt1 = new TestType1();
      tt1.ReturnSelf((err, result) => {
        assert.ok(!err);
        assert.equal(result._referenceId, tt1._referenceId);
        done();
      });
    });

    // Note: synchronous support not present atm
    // it('can return itself synchronously', () => {
      // var tt1 = new TestType1();
      // var copy = tt1.ReturnSelf(true);
      // assert.equal(copy._referenceId, tt1._referenceId);
    // });
  });

  describe('#ReturnTarget', () => {
    it('returns the target successfully', (done) => {
      var tt1 = new TestType1();
      var tt2 = new TestType2();

      tt1.ReturnTarget(tt2, (err, result) => {
        assert.ok(!err);
        assert.equal(result._referenceId, tt2._referenceId);
        done();
      });
    });
  });

});
