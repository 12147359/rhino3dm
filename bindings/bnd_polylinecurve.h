#include "bindings.h"

#pragma once

class BND_PolylineCurve : public BND_Curve
{
  std::shared_ptr<ON_PolylineCurve> m_polylinecurve;
public:
  BND_PolylineCurve();
  BND_PolylineCurve(ON_PolylineCurve* polylinecurve);
  int PointCount() const;
  ON_3dPoint Point(int index) const;
};
