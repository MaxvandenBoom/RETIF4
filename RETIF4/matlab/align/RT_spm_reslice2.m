function [reslicedData] = RT_spm_reslice2(memAlignObject, volMat, volDim, volData)
    
    flags = [];
    %flags.interp = 1;   % trilinear
    flags.interp = 2;   % 2nd degree spline
    %flags.interp = 4;   % 4th degree spline
    flags.wrap = [0 0 0];
    flags.mask = 1;

    %-Reslice
    %--------------------------------------------------------------------------

    % Reslice images volume by volume
    % FORMAT reslice_images(P,flags)
    % See main function for a description of the input parameters

    if ~isfinite(flags.interp), % Use Fourier method
        % Check for non-rigid transformations in the matrixes

        pp = memAlignObject.mat \ volMat;
        if any(abs(svd(pp(1:3,1:3))-1)>1e-7)
            fprintf('\n  Zooms  or shears  appear to  be needed');
            fprintf('\n  (probably due to non-isotropic voxels).');
            fprintf('\n  These  can not yet be  done  using  the');
            fprintf('\n  Fourier reslicing method.  Switching to');
            fprintf('\n  7th degree B-spline interpolation instead.\n\n');
            flags.interp = 7;
        end
        
    end

    if flags.mask
        x1    = repmat((1:memAlignObject.dim(1))', 1, memAlignObject.dim(2));
        x2    = repmat( 1:memAlignObject.dim(2)  , memAlignObject.dim(1), 1);
        if flags.mask, msk = cell(memAlignObject.dim(3),1);  end;
        for x3 = 1:memAlignObject.dim(3)
            tmp = zeros(memAlignObject.dim(1:2));
            tmp = tmp + getmask(inv(memAlignObject.mat\memAlignObject.mat),x1,x2,x3,volDim(1:3),flags.wrap);
            tmp = tmp + getmask(inv(memAlignObject.mat\volMat),x1,x2,x3,volDim(1:3),flags.wrap);
            if flags.mask, msk{x3} = find(tmp ~= 2); end;
        end
    end

    [x1,x2] = ndgrid(1:memAlignObject.dim(1),1:memAlignObject.dim(2));
    d       = [flags.interp*[1 1 1]' flags.wrap(:)];

    if ~isfinite(flags.interp)
        reslicedData = abs(kspace3d(spm_bsplinc(volData, [0 0 0 ; 0 0 0]'), memAlignObject.mat\volMat));
        for x3 = 1:memAlignObject.dim(3)
            if flags.mask
                tmp = reslicedData(:,:,x3); tmp(msk{x3}) = NaN; reslicedData(:,:,x3) = tmp;
            end
        end
    else
        C = spm_bsplinc(volData, d);
        reslicedData = zeros(memAlignObject.dim);
        for x3 = 1:memAlignObject.dim(3)
            [tmp,y1,y2,y3] = getmask(inv(memAlignObject.mat\volMat),x1,x2,x3,volDim(1:3),flags.wrap);
            reslicedData(:,:,x3)      = spm_bsplins(C, y1,y2,y3, d);
            % v(~tmp)      = 0;

            if flags.mask
                tmp = reslicedData(:,:,x3); tmp(msk{x3}) = NaN; reslicedData(:,:,x3) = tmp;
            end
        end
    end
    
end

%==========================================================================
%-function v = kspace3d(v,M)
%==========================================================================
function v = kspace3d(v,M)
    % 3D rigid body transformation performed as shears in 1D Fourier space
    % FORMAT v = kspace3d(v,M)
    % v        - image stored as a 3D array
    % M        - rigid body transformation matrix
    %
    % v        - transformed image
    %
    % References:
    % R. W. Cox and A. Jesmanowicz (1999)
    % Real-Time 3D Image Registration for Functional MRI
    % Magnetic Resonance in Medicine 42(6):1014-1018
    %
    % W. F. Eddy, M. Fitzgerald and D. C. Noll (1996)
    % Improved Image Registration by Using Fourier Interpolation
    % Magnetic Resonance in Medicine 36(6):923-931

    [S0,S1,S2,S3] = shear_decomp(M);

    d  = [size(v) 1 1 1];
    g = 2.^ceil(log2(d));
    if any(g~=d)
        tmp = v;
        v   = zeros(g);
        v(1:d(1),1:d(2),1:d(3)) = tmp;
        clear tmp;
    end

    % XY-shear
    tmp1 = -sqrt(-1)*2*pi*([0:((g(3)-1)/2) 0 (-g(3)/2+1):-1])/g(3);
    for j=1:g(2)
        t        = reshape( exp((j*S3(3,2) + S3(3,1)*(1:g(1)) + S3(3,4)).'*tmp1) ,[g(1) 1 g(3)]);
        v(:,j,:) = real(ifft(fft(v(:,j,:),[],3).*t,[],3));
    end

    % XZ-shear
    tmp1 = -sqrt(-1)*2*pi*([0:((g(2)-1)/2) 0 (-g(2)/2+1):-1])/g(2);
    for k=1:g(3)
        t        = exp( (k*S2(2,3) + S2(2,1)*(1:g(1)) + S2(2,4)).'*tmp1);
        v(:,:,k) = real(ifft(fft(v(:,:,k),[],2).*t,[],2));
    end

    % YZ-shear
    tmp1 = -sqrt(-1)*2*pi*([0:((g(1)-1)/2) 0 (-g(1)/2+1):-1])/g(1);
    for k=1:g(3)
        t        = exp( tmp1.'*(k*S1(1,3) + S1(1,2)*(1:g(2)) + S1(1,4)));
        v(:,:,k) = real(ifft(fft(v(:,:,k),[],1).*t,[],1));
    end

    % XY-shear
    tmp1 = -sqrt(-1)*2*pi*([0:((g(3)-1)/2) 0 (-g(3)/2+1):-1])/g(3);
    for j=1:g(2)
        t        = reshape( exp( (j*S0(3,2) + S0(3,1)*(1:g(1)) + S0(3,4)).'*tmp1) ,[g(1) 1 g(3)]);
        v(:,j,:) = real(ifft(fft(v(:,j,:),[],3).*t,[],3));
    end

    if any(g~=d), v = v(1:d(1),1:d(2),1:d(3)); end
    
end

%==========================================================================
%-function [S0,S1,S2,S3] = shear_decomp(A)
%==========================================================================
function [S0,S1,S2,S3] = shear_decomp(A)
    % Decompose rotation and translation matrix A into shears S0, S1, S2 and
    % S3, such that A = S0*S1*S2*S3. The original procedure is documented in:
    % R. W. Cox and A. Jesmanowicz (1999)
    % Real-Time 3D Image Registration for Functional MRI
    % Magnetic Resonance in Medicine 42(6):1014-1018

    A0 = A(1:3,1:3);
    if any(abs(svd(A0)-1)>1e-7), error('Can''t decompose matrix'); end

    t  = A0(2,3); if t==0, t=eps; end
    a0 = pinv(A0([1 2],[2 3])')*[(A0(3,2)-(A0(2,2)-1)/t) (A0(3,3)-1)]';
    S0 = [1 0 0; 0 1 0; a0(1) a0(2) 1];
    A1 = S0\A0;  a1 = pinv(A1([2 3],[2 3])')*A1(1,[2 3])';  S1 = [1 a1(1) a1(2); 0 1 0; 0 0 1];
    A2 = S1\A1;  a2 = pinv(A2([1 3],[1 3])')*A2(2,[1 3])';  S2 = [1 0 0; a2(1) 1 a2(2); 0 0 1];
    A3 = S2\A2;  a3 = pinv(A3([1 2],[1 2])')*A3(3,[1 2])';  S3 = [1 0 0; 0 1 0; a3(1) a3(2) 1];

    s3 = A(3,4)-a0(1)*A(1,4)-a0(2)*A(2,4);
    s1 = A(1,4)-a1(1)*A(2,4);
    s2 = A(2,4);
    S0 = [[S0 [0  0 s3]'];[0 0 0 1]];
    S1 = [[S1 [s1 0  0]'];[0 0 0 1]];
    S2 = [[S2 [0 s2  0]'];[0 0 0 1]];
    S3 = [[S3 [0  0  0]'];[0 0 0 1]];
    
end

%==========================================================================
%-function [Mask,y1,y2,y3] = getmask(M,x1,x2,x3,dim,wrp)
%==========================================================================
function [Mask,y1,y2,y3] = getmask(M,x1,x2,x3,dim,wrp)
    tiny = 5e-2; % From spm_vol_utils.c
    y1   = M(1,1)*x1+M(1,2)*x2+(M(1,3)*x3+M(1,4));
    y2   = M(2,1)*x1+M(2,2)*x2+(M(2,3)*x3+M(2,4));
    y3   = M(3,1)*x1+M(3,2)*x2+(M(3,3)*x3+M(3,4));
    Mask = true(size(y1));
    if ~wrp(1), Mask = Mask & (y1 >= (1-tiny) & y1 <= (dim(1)+tiny)); end
    if ~wrp(2), Mask = Mask & (y2 >= (1-tiny) & y2 <= (dim(2)+tiny)); end
    if ~wrp(3), Mask = Mask & (y3 >= (1-tiny) & y3 <= (dim(3)+tiny)); end
end
