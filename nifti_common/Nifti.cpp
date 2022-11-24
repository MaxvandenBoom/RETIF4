#ifdef WIN64
#define BIT_VERSION 64
#else
#define BIT_VERSION 32
#endif

#include "Nifti.h"
#include "nifti_helper.hpp"
#include <armadillo>
#include "nifti1.h"
#include "nifti1_io.h"
#include <algorithm>


int n_GetBitVersion() {	
	return BIT_VERSION;	
}

bool n_ReadNifti_Info(const char* filename, int* nx, int* ny, int* nz, long* nvox, int* nbyper, int* ndim, int* datatype, double* affine) {

	// read the image (only the information, not the data)
	nifti_image* image = nifti_image_read(filename, 0);
	if (image == NULL) {
		std::cout << "NULL image returned: " << filename << std::endl;
		return false;

	} else {

		// check endianness
		if (image->byteorder != nifti_short_order() && image->datatype != DT_UINT8) {
			nifti_image_free(image);
			return false;
		}

		// affine transform
		arma::mat image_affine;
		if (image->sform_code > 0) {
			
			image_affine = arma::conv_to<arma::mat>::from(arma::fmat(*(image->sto_xyz).m, 4, 4)).t();
			
			convZeroToOne(&image_affine);
			rescaleMat(&image_affine, image->xyz_units, 0);
			
			for (int i = 0; i < 16; i++)	affine[i] = image_affine[i];

		} else if (image->qform_code > 0) {
			
			decode_qform0(&image_affine, image);
			rescaleMat(&image_affine, image->xyz_units, 0);

			for (int i = 0; i < 16; i++)	affine[i] = image_affine[i];

		} else {
			nifti_image_free(image);
			return false;
		}
		
		// transfer the nifti information
		*nx = image->nx;
		*ny = image->ny;
		*nz = image->nz;
		*nvox = image->nvox;
		*nbyper = image->nbyper;
		*ndim = 3;
		*datatype = image->datatype;

		// free the image memory and return success
		nifti_image_free(image);
		return true;

	}
		
}

bool n_ReadNifti_Data_char(const char* filename, unsigned char* dstDataBuffer, long bufferLength) {

	// read the image (also the data)
	nifti_image* image = nifti_image_read(filename, 1);
	if (image == NULL) {
		return false;
	} else {

		// check endianness
		if (image->byteorder != nifti_short_order() && image->datatype != DT_UINT8) {
			nifti_image_free(image);
			return false;
		}

		// check if the lengths of the image buffer and the target buffer match
		if (image->nvox * image->nbyper != bufferLength) {
			return false;
		}

		// copy the image data into the managed buffer (ref coming from c# call)
		memcpy(dstDataBuffer, (unsigned char*)image->data, bufferLength);

		// free the image data
		nifti_image_free(image);
		
		// return success
		return true;

	}

}


bool n_ReadNifti_Data_ushort(const char* filename, unsigned short* dstDataBuffer, long bufferLength) {

	// read the image (also the data)
	nifti_image* image = nifti_image_read(filename, 1);
	if (image == NULL) {		
		return false;
	} else {

		// check endianness
		if (image->byteorder != nifti_short_order() && image->datatype != DT_UINT8) {
			nifti_image_free(image);
			return false;
		}

		// check if the lengths of the image buffer and the target buffer match
		if (image->nvox * image->nbyper != bufferLength) {
			return false;
		}

		// copy the image data into the managed buffer (ref coming from c# call)
		memcpy(dstDataBuffer, (unsigned char*)image->data, bufferLength);

		// free the image data and return success
		nifti_image_free(image);
		return true;

	}

}

int n_test() {

	//nifti_image* image = nifti_image_read("D:\\ch2bet.nii", 1);
	//arma::cube rr = memToCharCube(((unsigned char *)image->data), image->nx, image->ny, image->nz);

	return 0;
}


int detrendTest(double* srcMatrix, double* dstMatrix, int lenDim0, int lenDim1) {
	
	/*
	for (int i = 0; i < 100; i++) {
		dstMatrix[i] = srcMatrix[i];
	}
	*/
	
	arma::mat image_affine = arma::mat(srcMatrix, lenDim0, lenDim1).t();
	image_affine[0] = 111;

	arma::mat B = arma::inv(image_affine);

	//double *dstMatrix = image_affine.memptr();
	int length = lenDim0 * lenDim1;
	for (int i = 0; i < length; i++) {
			dstMatrix[i] = B[i];
	}

	//float *a = A.memptr();
	


	return 43210;
}
