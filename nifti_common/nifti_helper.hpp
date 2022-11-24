#ifndef __NIFTIHELPER_H_
#define __NIFTIHELPER_H_

#include <armadillo>
extern "C" {
#include "nifti1.h"
#include "nifti1_io.h"
}

using namespace arma;

void convZeroToOne(mat* matrix);
void convOneToZero(mat* matrix);
void q2m(mat* m, double qb, double qc, double qd);
void decode_qform0(mat* outM, nifti_image* image);
void rescaleMat(mat* matrix, int unit, int rescaleto);

#endif /* __NIFTIHELPER_H_ */
