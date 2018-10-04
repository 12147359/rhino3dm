#include "bindings.h"

BND_Geometry::BND_Geometry()
{
}

BND_Geometry::BND_Geometry(ON_Geometry* geometry, const ON_ModelComponentReference* compref)
{
  SetTrackedPointer(geometry, compref);
}

void BND_Geometry::SetTrackedPointer(ON_Geometry* geometry, const ON_ModelComponentReference* compref)
{
  m_geometry = geometry;
  BND_Object::SetTrackedPointer(geometry, compref);
}

int BND_Geometry::Dimension() const
{
  return m_geometry->Dimension();
}

BND_BoundingBox BND_Geometry::BoundingBox() const
{
  ON_BoundingBox bbox = m_geometry->BoundingBox();
  return BND_BoundingBox(bbox);
}

bool BND_Geometry::Rotate(double rotation_angle, const ON_3dVector& rotation_axis, const ON_3dPoint& rotation_center)
{
  return m_geometry->Rotate(rotation_angle, rotation_axis, rotation_center);
}

#if defined(ON_PYTHON_COMPILE)
namespace py = pybind11;
void initGeometryBindings(pybind11::module& m)
{
  py::class_<BND_Geometry, BND_Object>(m, "GeometryBase")
    .def("GetBoundingBox", &BND_Geometry::BoundingBox)
    .def("Rotate", &BND_Geometry::Rotate);
}
#endif
