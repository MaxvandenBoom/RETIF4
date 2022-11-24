#include "nifti_helper.hpp"

void convZeroToOne(mat* matrix) {
	mat c;
	c << 1 << 0 << 0 << -1 << endr << 0 << 1 << 0 << -1 << endr << 0 << 0 << 1 << -1 << endr << 0 << 0 << 0 << 1 << endr;
	(*matrix) *= c;
}

void convOneToZero(mat* matrix) {
	mat c;
	c << 1 << 0 << 0 << 1 << endr << 0 << 1 << 0 << 1 << endr << 0 << 0 << 1 << 1 << endr << 0 << 0 << 0 << 1 << endr;
	(*matrix) *= c;
}

void q2m(mat* m, double qb, double qc, double qd) {
	double w = sqrt(1 - (qb * qb + qc * qc + qd * qd));
	double x = qb;
	double y = qc;
	double z = qd;
	if (w < 1e-7 || _isnan(w)) {
		w = 1 / sqrt(x * x + y * y + z * z);
		x *= w;
		y *= w;
		z *= w;
		w = 0;
	}
	double xx = x * x;
	double yy = y * y;
	double zz = z * z;
	double ww = w * w;

	double xy = x * y;
	double xz = x * z;
	double xw = x * w;

	double yz = y * z;
	double yw = y * w;

	double zw = z * w;

	(*m).zeros(4, 4);
	(*m)(0, 0) = (xx - yy - zz + ww);
	(*m)(0, 1) = 2 * (xy - zw);
	(*m)(0, 2) = 2 * (xz + yw);
	(*m)(0, 3) = 0;

	(*m)(1, 0) = 2 * (xy + zw);
	(*m)(1, 1) = (-xx + yy - zz + ww);
	(*m)(1, 2) = 2 * (yz - xw);
	(*m)(1, 3) = 0;

	(*m)(2, 0) = 2 * (xz - yw);
	(*m)(2, 1) = 2 * (yz + xw);
	(*m)(2, 2) = (-xx - yy + zz + ww);
	(*m)(2, 3) = 0;

	(*m)(3, 0) = 0;
	(*m)(3, 1) = 0;
	(*m)(3, 2) = 0;
	(*m)(3, 3) = 1;
}

void decode_qform0(mat* outM, nifti_image* image) {
  
	if (image->qform_code <= 0) {
		throw std::invalid_argument("qform <= 0, not supported");
	} else {
		mat r;
		q2m(&r, image->quatern_b, image->quatern_c, image->quatern_d);

		mat t = eye(4, 3);
		colvec tvec;
		tvec << image->qoffset_x << image->qoffset_y << image->qoffset_z << 1;
		t.insert_cols(3, tvec);

		int n = std::min(image->dim[0], 3);
		mat z = ones(1, 4);
		for (int i = 0; i < n; i++)
			z(0, i) = image->pixdim[1 + i] < 0 ? 1 : image->pixdim[i + 1];

		if (image->qfac < 0)
			z(2) *= -1;
		z = diagmat(z);

		(*outM) = t * r * z;
	}
	convZeroToOne(outM);
}

void rescaleMat(mat* matrix, int unit, int rescaleto) {
	double rescale;
	switch (unit) {
		case NIFTI_UNITS_METER:
			rescale = 1000;
			return;

		case NIFTI_UNITS_MM:
			rescale = 1;
			break;

		case NIFTI_UNITS_MICRON:
			rescale = 0.001;
			return;

		default:
			return;
	}

	vec diagvec;
	if (rescaleto == 1) {
		diagvec << rescale << rescale << rescale << 1 << endr;
	} else {
		diagvec << 1 / rescale << 1 / rescale << 1 / rescale << 1 << endr;
	}
	*matrix *= diagmat(diagvec);
}
