function outMat = reorient3D(inMat, index)
    switch index
        case 1    
            outMat = flip(inMat, 1);
        case 2
            outMat = flip(inMat, 2);
        case 3
            outMat = flip(inMat, 3);
        case 4
            outMat = flip(flip(inMat, 1), 2);
        case 5
            outMat = flip(flip(inMat, 2), 3);
        case 6
            outMat = flip(flip(inMat, 1), 3);
        case 7
            outMat = flip(flip(flip(inMat, 1), 2), 3);
        case 8
            outMat = rot90_3D_On4DMat(inMat, 1, 1);
        case 9
            outMat = flip(rot90_3D_On4DMat(inMat, 1, 1), 1);
        case 10
            outMat = flip(rot90_3D_On4DMat(inMat, 1, 1), 2);
        case 11
            outMat = flip(rot90_3D_On4DMat(inMat, 1, 1), 3);
        case 12
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 1, 1), 1), 2);
        case 13
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 1, 1), 2), 3);
        case 14
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 1, 1), 1), 3);
        case 15
            outMat = flip(flip(flip(rot90_3D_On4DMat(inMat, 1, 1), 1), 2), 3);
        case 16
            outMat = rot90_3D_On4DMat(inMat, 2, 1);
        case 17
            outMat = flip(rot90_3D_On4DMat(inMat, 2, 1), 1);
        case 18
            outMat = flip(rot90_3D_On4DMat(inMat, 2, 1), 2);
        case 19
            outMat = flip(rot90_3D_On4DMat(inMat, 2, 1), 3);
        case 20
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 2, 1), 1), 2);
        case 21
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 2, 1), 2), 3);
        case 22
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 2, 1), 1), 3);
        case 23
            outMat = flip(flip(flip(rot90_3D_On4DMat(inMat, 2, 1), 1), 2), 3);
        case 24
            outMat = rot90_3D_On4DMat(inMat, 3, 1);
        case 25
            outMat = flip(rot90_3D_On4DMat(inMat, 3, 1), 1);
        case 26
            outMat = flip(rot90_3D_On4DMat(inMat, 3, 1), 2);
        case 27
            outMat = flip(rot90_3D_On4DMat(inMat, 3, 1), 3);
        case 28
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 3, 1), 1), 2);
        case 29
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 3, 1), 2), 3);
        case 30
            outMat = flip(flip(rot90_3D_On4DMat(inMat, 3, 1), 1), 3);
        case 31
            outMat = flip(flip(flip(rot90_3D_On4DMat(inMat, 3, 1), 1), 2), 3);
        case 32
            outMat = rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1);
        case 33
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 1);
        case 34
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 2);
        case 35
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 3);
        case 36
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 1), 2);
        case 37
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 2), 3);
        case 38
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 1), 3);
        case 39
            outMat = flip(flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 2, 1), 1), 2), 3);
        case 40
            outMat = rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1);
        case 41
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 1);
        case 42
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 2);
        case 43
            outMat = flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 3);
        case 44
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 1), 2);
        case 45
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 2), 3);
        case 46
            outMat = flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 1), 3);
        case 47
            outMat = flip(flip(flip(rot90_3D_On4DMat(rot90_3D_On4DMat(inMat, 1, 1), 3, 1), 1), 2), 3);
        otherwise
            outMat = inMat;
    end
end
