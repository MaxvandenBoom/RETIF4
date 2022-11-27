function [outVolMat, Q] = RT_spm_realign_mem2(memAlignObject, volMat, volData)

    % set flags
    flags = [];
    flags.quality = 0.9;
    flags.fwhm = 5;
    flags.sep = 4;
    %flags.interp = 1;   % trilinear
    flags.interp = 2;   % 2nd degree spline
    flags.wrap = [0 0 0];
    
    % transfer relevant variables from the mem align object
    deg = memAlignObject.deg;
    A0 = memAlignObject.A0;
    b = memAlignObject.b;
    x1 = memAlignObject.x1;
    x2 = memAlignObject.x2;
    x3 = memAlignObject.x3;
    
    % needed variable
    lkp  = 1:6;
    
    % realign the image
    V  = smooth_vol(volMat,volData,flags.interp,flags.wrap,flags.fwhm);
    d  = [size(V) 1 1];
    d  = d(1:3);
    ss = Inf;
    countdown = -1;
    for iter=1:64
        [y1,y2,y3] = coords([0 0 0  0 0 0], memAlignObject.P(1).mat, volMat,x1,x2,x3);
        msk        = find((y1>=1 & y1<=d(1) & y2>=1 & y2<=d(2) & y3>=1 & y3<=d(3)));
        if length(msk)<32, error_message(); end

        F          = spm_bsplins(V, y1(msk),y2(msk),y3(msk),deg);

        A          = A0(msk,:);
        b1         = b(msk);
        sc         = sum(b1)/sum(F);
        b1         = b1-F*sc;
        soln       = (A'*A)\(A'*b1);

        p          = [0 0 0  0 0 0  1 1 1  0 0 0];
        p(lkp)     = p(lkp) + soln';
        volMat     = spm_matrix(p) \ volMat;

        pss        = ss;
        ss         = sum(b1.^2)/length(b1);
        if (pss-ss)/pss < 1e-8 && countdown == -1 % Stopped converging.
            countdown = 2;
        end
        if countdown ~= -1
            if countdown==0, break; end
            countdown = countdown -1;
        end
    end
    
    % return the correction parameters
    Q = spm_imatrix(volMat / memAlignObject.P(1).mat);
    Q = Q(1:6);
    
    % set the realigned volume matrix as the output matrix 
    outVolMat = volMat;
end

%==========================================================================
% function [y1,y2,y3]=coords(p,M1,M2,x1,x2,x3)
%==========================================================================
function [y1,y2,y3]=coords(p,M1,M2,x1,x2,x3)
    % Rigid body transformation of a set of coordinates
    M  = inv(M2) * inv(spm_matrix(p)) * M1;
    y1 = M(1,1)*x1 + M(1,2)*x2 + M(1,3)*x3 + M(1,4);
    y2 = M(2,1)*x1 + M(2,2)*x2 + M(2,3)*x3 + M(2,4);
    y3 = M(3,1)*x1 + M(3,2)*x2 + M(3,3)*x3 + M(3,4);
end

%==========================================================================
% function V = smooth_vol(P,hld,wrp,fwhm)
%==========================================================================
function V = smooth_vol(volMat, volData, hld, wrp, fwhm)
    % Convolve the volume in memory
    s  = sqrt(sum(volMat(1:3,1:3).^2)).^(-1)*(fwhm/sqrt(8*log(2)));
    x  = round(6*s(1)); x = -x:x;
    y  = round(6*s(2)); y = -y:y;
    z  = round(6*s(3)); z = -z:z;
    x  = exp(-(x).^2/(2*(s(1)).^2));
    y  = exp(-(y).^2/(2*(s(2)).^2));
    z  = exp(-(z).^2/(2*(s(3)).^2));
    x  = x/sum(x);
    y  = y/sum(y);
    z  = z/sum(z);

    i  = (length(x) - 1)/2;
    j  = (length(y) - 1)/2;
    k  = (length(z) - 1)/2;
    d  = [hld*[1 1 1]' wrp(:)];
    V  = spm_bsplinc(volData, d);
    spm_conv_vol(V,V,x,y,z,-[i j k]);
end

%==========================================================================
% function error_message()
%==========================================================================
function error_message()
    str = {'There is not enough overlap in the images to obtain a solution.',...
           ' ',...
           'Please check that your header information is OK.',...
           'The Check Reg utility will show you the initial',...
           'alignment between the images, which must be',...
           'within about 4cm and about 15 degrees in order',...
           'for SPM to find the optimal solution.'};
    spm('alert*',str,mfilename,sqrt(-1));
    error('Insufficient image overlap.');
end

