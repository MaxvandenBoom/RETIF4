function [outVolData] = RT_spm_smooth_mem(volMat, volData, fwhm)

    s = [fwhm, fwhm, fwhm];
    VOX = sqrt(sum(volMat(1:3,1:3).^2));

    %-Compute parameters for spm_conv_vol
    %--------------------------------------------------------------------------
    s  = s./VOX;                        % voxel anisotropy
    s1 = s/sqrt(8*log(2));              % FWHM -> Gaussian parameter

    x  = round(6*s1(1)); x = -x:x; x = spm_smoothkern(s(1),x,1); x  = x/sum(x);
    y  = round(6*s1(2)); y = -y:y; y = spm_smoothkern(s(2),y,1); y  = y/sum(y);
    z  = round(6*s1(3)); z = -z:z; z = spm_smoothkern(s(3),z,1); z  = z/sum(z);

    i  = (length(x) - 1)/2;
    j  = (length(y) - 1)/2;
    k  = (length(z) - 1)/2;
    
    outVolData = zeros(size(volData));
    spm_conv_vol(volData, outVolData, x, y, z, -[i,j,k]);
    
end
