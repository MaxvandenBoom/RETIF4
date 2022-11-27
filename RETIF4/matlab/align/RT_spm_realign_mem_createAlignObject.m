function [memAlignObject] = RT_spm_realign_mem_createAlignObject(P)

    % set flags
    flags = [];
    flags.quality = 0.9;
    flags.fwhm = 5;
    flags.sep = 4;
    flags.interp = 2;   % 2nd degree spline
    flags.wrap = [0 0 0];
    
    % create empty align object
    memAlignObject = [];

    skip = sqrt(sum(P(1).mat(1:3,1:3).^2)).^(-1)*flags.sep;
    d    = P(1).dim(1:3);                                                                                                                        
    lkp  = 1:6;
    st   = rand('state'); % st = rng;
    rand('state',0); % rng(0,'v5uniform'); % rng('defaults');
    if d(3) < 3
        lkp  = [1 2 6];
        [x1,x2,x3] = ndgrid(1:skip(1):d(1)-.5, 1:skip(2):d(2)-.5, 1:skip(3):d(3));
        x1   = x1 + rand(size(x1))*0.5;
        x2   = x2 + rand(size(x2))*0.5;
    else
        [x1,x2,x3] = ndgrid(1:skip(1):d(1)-.5, 1:skip(2):d(2)-.5, 1:skip(3):d(3)-.5);
        x1   = x1 + rand(size(x1))*0.5;
        x2   = x2 + rand(size(x2))*0.5;
        x3   = x3 + rand(size(x3))*0.5; 
    end
    rand('state',st); % rng(st);

    memAlignObject.x1 = x1(:);
    memAlignObject.x2 = x2(:);
    memAlignObject.x3 = x3(:);

    %-Compute rate of change of chi2 w.r.t changes in parameters (matrix A)
    %--------------------------------------------------------------------------
    V   = smooth_vol(P(1),flags.interp,flags.wrap,flags.fwhm);
    memAlignObject.deg = [flags.interp*[1 1 1]' flags.wrap(:)];

    [G,dG1,dG2,dG3] = spm_bsplins(  V, ...
                                    memAlignObject.x1, memAlignObject.x2, memAlignObject.x3, ...
                                    memAlignObject.deg);
    clear V
    memAlignObject.A0  = make_A(P(1).mat, ...
                                memAlignObject.x1, memAlignObject.x2, memAlignObject.x3, ...
                                dG1, dG2, dG3, lkp);
    
    b   = G;
    memAlignObject.b    = b;
    memAlignObject.P(1) = P(1);
    memAlignObject.mat = P(1).mat;
    memAlignObject.dim = P(1).dim;
    
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
function V = smooth_vol(P,hld,wrp,fwhm)
    % Convolve the volume in memory
    s  = sqrt(sum(P.mat(1:3,1:3).^2)).^(-1)*(fwhm/sqrt(8*log(2)));
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
    V  = spm_bsplinc(P,d);
    spm_conv_vol(V,V,x,y,z,-[i j k]);
end

%==========================================================================
% function A = make_A(M,x1,x2,x3,dG1,dG2,dG3,lkp)
%==========================================================================
function A = make_A(M,x1,x2,x3,dG1,dG2,dG3,lkp)
    % Matrix of rate of change of weighted difference w.r.t. parameter changes
    p0 = [0 0 0  0 0 0  1 1 1  0 0 0];
    A  = zeros(numel(x1),length(lkp));
    for i=1:length(lkp)
        pt         = p0;
        pt(lkp(i)) = pt(i)+1e-6;
        [y1,y2,y3] = coords(pt,M,M,x1,x2,x3);
        tmp        = sum([y1-x1 y2-x2 y3-x3].*[dG1 dG2 dG3],2)/(-1e-6);
        A(:,i) = tmp;
    end
end


