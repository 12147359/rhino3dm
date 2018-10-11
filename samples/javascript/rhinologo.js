const COMPUTE_URL_BASE = "https://compute.rhino3d.com/Rhino/";
var getAuthToken = null;

class RhinoLogoDoc {
  constructor(model) {
    this.model = model;
    this.breps = [];
    var objecttable = model.objects();
    for(var i=0; i<objecttable.count; i++) {
      var modelobject = objecttable.get(i);
      this.breps.push({"geometry":modelobject.geometry(), "meshes":[], "wires":[], "threejs":null, "threejswires":null});
    }
  }

  computeBrepMeshes(callback) {
    for(var i=0; i<this.breps.length; i++) {
      var brep = this.breps[i]["geometry"];
      var functionArgs = [brep.encode()];
      var auth = getAuthToken();

      const fetchFunc = (m, index, args, auth) => {
        fetch(COMPUTE_URL_BASE + "Geometry/Mesh/CreateFromBrep", {
          "method":"POST",
          "body": JSON.stringify(args),
          "headers": {"Authorization":auth}
        })
        .then(r=>r.json())
        .then(result=>{
          var meshes = result.map(r=>Module.CommonObject.decode(r));
          m.breps[index]["meshes"] = meshes;
          callback(this);
        });
      };
      fetchFunc(this, i, functionArgs, auth);
    }
  }

  computeBrepWires(callback) {
    for(var i=0; i<this.breps.length; i++) {
      var brep = this.breps[i]["geometry"];
      var functionArgs = [brep.encode(), 1];
      var auth = getAuthToken();

      const fetchFunc = (m, index, args, auth) => {
        fetch(COMPUTE_URL_BASE + "Geometry/Brep/GetWireFrame", {
          "method":"POST",
          "body": JSON.stringify(args),
          "headers": {"Authorization":auth}
        })
        .then(r=>r.json())
        .then(result=>{
          var curves = result.map(r=>Module.CommonObject.decode(r));
          m.breps[index]["wires"] = curves;
          callback(this);
        });
      };
      fetchFunc(this, i, functionArgs, auth);
    }
  }
};


function brepToMesh(brep) {
  functionArgs = [brep.encode()];
  auth = getAuthToken();

  fetch(COMPUTE_URL_BASE + "Geometry/Mesh/CreateFromBrep", {
    "method":"POST",
    "body": JSON.stringify(functionArgs),
    "headers": {"Authorization":auth}
  })
  .then(r=>r.json())
  .then(result=>{
    meshes = result.map(r=>Module.CommonObject.decode(r));

    for( i=0; i<meshes.length; i++ ) {
      addMesh(meshes[i]);
    }
    draw3dMeshes();
  });
}

/**
 * Loads the 'Rhino Logo.3dm' model and computes meshes for each of
 * breps in the model.
 *
 * @param (function) onModelLoadedCallback - function to call once
 *     the model has been initially downloaded and converted into a class
 * @param (function) onBrepMeshedCallback - function to call each
 *     time meshes are computed for a brep
 */
function getRhinoLogoMeshes(onModelLoadedCallback=null, onBrepMeshedCallback=null, onBrepWiresCallback=null) {
  req = new XMLHttpRequest();
  req.open("GET", "https://files.mcneel.com/TEST/Rhino Logo.3dm");
  req.responseType = "arraybuffer";
  req.addEventListener("loadend", loadEnd);
  req.send(null);

  function loadEnd(e) {
    longInt8View = new Uint8Array(req.response);
    var model = Module.File3dm.fromByteArray(longInt8View);
    var doc = new RhinoLogoDoc(model);
    if( onModelLoadedCallback!=null )
      onModelLoadedCallback(doc);
    if( onBrepWiresCallback!=null )
      doc.computeBrepWires(onBrepWiresCallback);
    if( onBrepMeshedCallback!=null )
      doc.computeBrepMeshes(onBrepMeshedCallback);
  }
}
