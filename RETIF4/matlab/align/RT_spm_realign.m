function [P, Q] = RT_spm_realign(P)

    flags = [];
    flags.quality = 0.9;
    flags.fwhm = 5;
    flags.sep = 4;
    flags.interp = 2;   % 2nd degree spline
    flags.wrap = [0 0 0];
    flags.lkp = 1:6;

    %-Perform realignment
    %==========================================================================
    % Realign a time series of 3D images to the first of the series
    % FORMAT P = realign_series(P,flags)
    % P  - a vector of volumes (see spm_vol)
    %--------------------------------------------------------------------------
    % P(i).mat is modified to reflect the modified position of the image i.
    % The scaling (and offset) parameters are also set to contain the
    % optimum scaling required to match the images.
    %__________________________________________________________________________

    if numel(P)<2, return; end

    skip = sqrt(sum(P(1).mat(1:3,1:3).^2)).^(-1)*flags.sep;
    d    = P(1).dim(1:3);                                                                                                                        
    lkp  = flags.lkp;
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

    x1 = x1(:);
    x2 = x2(:);
    x3 = x3(:);

    %-Compute rate of change of chi2 w.r.t changes in parameters (matrix A)
    %--------------------------------------------------------------------------
    V   = smooth_vol(P(1),flags.interp,flags.wrap,flags.fwhm);
    deg = [flags.interp*[1 1 1]' flags.wrap(:)];

    [G,dG1,dG2,dG3] = spm_bsplins(V,x1,x2,x3,deg);
    clear V
    A0  = make_A(P(1).mat,x1,x2,x3,dG1,dG2,dG3,lkp);

    b   = G;

    %-Remove voxels that contribute very little to the final estimate
    %--------------------------------------------------------------------------
    if numel(P) > 2
        % Simulated annealing or something similar could be used to
        % eliminate a better choice of voxels - but this way will do for
        % now. It basically involves removing the voxels that contribute
        % least to the determinant of the inverse covariance matrix.
        Alpha = [A0 b];
        Alpha = Alpha'*Alpha;
        det0  = det(Alpha);
        det1  = det0;
        while det1/det0 > flags.quality
            dets = zeros(size(A0,1),1);
            for i=1:size(A0,1)
                tmp     = [A0(i,:) b(i)];
                dets(i) = det(Alpha - tmp'*tmp);
            end
            clear tmp
            [~, msk] = sort(det1-dets);
            msk        = msk(1:round(length(dets)/10));
             A0(msk,:) = [];   b(msk,:) = [];   G(msk,:) = [];
             x1(msk,:) = [];  x2(msk,:) = [];  x3(msk,:) = [];
            dG1(msk,:) = []; dG2(msk,:) = []; dG3(msk,:) = [];
            Alpha = [A0 b];
            Alpha = Alpha'*Alpha;
            det1  = det(Alpha);
        end
    end

    %-Loop over images
    %--------------------------------------------------------------------------
    for i=2:numel(P)
        V  = smooth_vol(P(i),flags.interp,flags.wrap,flags.fwhm);
        d  = [size(V) 1 1];
        d  = d(1:3);
        ss = Inf;
        countdown = -1;
        for iter=1:64
            [y1,y2,y3] = coords([0 0 0  0 0 0],P(1).mat,P(i).mat,x1,x2,x3);
            msk        = find((y1>=1 & y1<=d(1) & y2>=1 & y2<=d(2) & y3>=1 & y3<=d(3)));
            if length(msk)<32, error_message(P(i)); end

            F          = spm_bsplins(V, y1(msk),y2(msk),y3(msk),deg);

            A          = A0(msk,:);
            b1         = b(msk);
            sc         = sum(b1)/sum(F);
            b1         = b1-F*sc;
            soln       = (A'*A)\(A'*b1);

            p          = [0 0 0  0 0 0  1 1 1  0 0 0];
            p(lkp)     = p(lkp) + soln';
            P(i).mat   = spm_matrix(p) \ P(i).mat;

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
    end
    
    

    % max: return the correction parameters
    n = length(P);
    Q = zeros(n,6);
    for j=1:n
        qq     = spm_imatrix(P(j).mat/P(1).mat);
        Q(j,:) = qq(1:6);
    end

    % max: this writes the changes to the header file
    % we want everything to happen in memory, so commented out
    
    %-Update voxel to world mapping in images header
    %------------------------------------------------------------------
    %for i=1:numel(P)
    %    spm_get_space([P(i).fname ',' num2str(P(i).n)], P(i).mat);
    %end    

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

%==========================================================================
% function error_message(P)
%==========================================================================
function error_message(P)
    str = {'There is not enough overlap in the images to obtain a solution.',...
           ' ',...
           'Offending image:',...
           P.fname,...
           ' ',...
           'Please check that your header information is OK.',...
           'The Check Reg utility will show you the initial',...
           'alignment between the images, which must be',...
           'within about 4cm and about 15 degrees in order',...
           'for SPM to find the optimal solution.'};
    spm('alert*',str,mfilename,sqrt(-1));
    error('Insufficient image overlap.');
end

