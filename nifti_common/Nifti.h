#ifdef NIFTI_EXPORTS
#define NIFTI_API __declspec(dllexport)
#else
#define NIFTI_API __declspec(dllimport)
#endif

#include <stdexcept>
using namespace std;

extern "C" { NIFTI_API int n_GetBitVersion(); }

extern "C" { NIFTI_API bool n_ReadNifti_Info(const char *filename, int* nx, int* ny, int* nz, long* nvox, int* nbyper, int* ndim, int* datatype, double* affine); }
extern "C" { NIFTI_API bool n_ReadNifti_Data_char(const char* filename, unsigned char* dstDataBuffer, long bufferLength); }
extern "C" { NIFTI_API bool n_ReadNifti_Data_ushort(const char* filename, unsigned short* dstDataBuffer, long bufferLength); }


extern "C" { NIFTI_API int n_test(); }
extern "C" { NIFTI_API int detrendTest(double* srcMatrix, double* dstMatrix, int lenDim0, int lenDim1); }
