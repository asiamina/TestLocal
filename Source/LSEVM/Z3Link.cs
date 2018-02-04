using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Z3;

namespace VSIXProjectThesis
{
    public class Z3Link
    {

        private Context z3Context;

        public Z3Link(){
            z3Context = new Context();
        }

        public void ProcessSyntaxTree(SyntaxTree syntaxTree){
            CompilationUnitSyntax root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            foreach (SyntaxNode node in root.DescendantNodes()){
                if (node is VariableDeclarationSyntax)
                    ProcessVariableDeclarationSyntax(node as VariableDeclarationSyntax);
                //else if (node is AssignmentExpressionSyntax)
                    //ProcessAssignmentExpressionSyntax(node as AssignmentExpressionSyntax);
                else if (node is IfStatementSyntax)
                    ProcessIfStatementSyntax( node as IfStatementSyntax);
            }

            
        }

        public IDictionary<string, List<object>> Solve(){
            Dictionary<string, List<object>> variableValues = new Dictionary<string, List<object>>(StringComparer.CurrentCultureIgnoreCase);
            new Model.ViewModelLocator().MainModel.AddDisplayText("Z3 Solve");
            Solver s = this.z3Context.MkSolver();
            foreach (BoolExpr sa in this.solverAssertions){
                s.Assert(sa);
            }
            
            Console.WriteLine(s.Check());
            new Model.ViewModelLocator().MainModel.AddDisplayText(s.Check().ToString());

            Microsoft.Z3.Model m = s.Model;
            foreach (FuncDecl d in m.Decls){
                string varName = d.Name.ToString();
                Expr valExpr = m.ConstInterp(d);
                string varValue = valExpr.ToString();
                if (valExpr is FPExpr){
                    
                    FPExpr fp = (FPExpr)valExpr;
                    FPNum fn = (FPNum)fp;
                    double val = Convert.ToDouble(fn.Significand);
                    val = val*System.Math.Pow(2,fn.ExponentInt64);
                    
                    if (fn.Sign)
                        val = val * -1;

                    varValue = val.ToString();

                    if(valExpr.ToString() == "+zero")
                        varValue = "0.01";
                    if (valExpr.ToString() == "-zero")
                        varValue = "-0.01";
                }

                
                string message = varName + " -> " + varValue;
                new Model.ViewModelLocator().MainModel.AddDisplayText(message);

                if (!variableValues.ContainsKey(varName))
                    variableValues.Add(varName, new List<object>());

                variableValues[varName].Add(varValue);

            }
            return variableValues;
        }
        private Queue<BoolExpr> solverAssertions = new Queue<BoolExpr>();
        private void ProcessIfStatementSyntax(IfStatementSyntax ifs){            
            
                ExpressionSyntax condition = ifs.Condition;
                BinaryExpressionSyntax bes = condition as BinaryExpressionSyntax;

            Expr z3Left = null, z3Right=null;
                if (bes != null)
                {
                    ExpressionSyntax es = bes.Left;
                    IdentifierNameSyntax ins = es as IdentifierNameSyntax;
                    string varName=null;
                    if (ins != null){                    
                        varName = ins.Identifier.Text;
                        z3Left = variableToZ3[varName];
                    }

                    es = bes.Right;
                    LiteralExpressionSyntax les = es as LiteralExpressionSyntax;
                    if (les != null){
                        string tokenText = les.Token.Text;
                        z3Right = GetExpr(varName, tokenText);
                    }

                BoolExpr boolExpr = null, boolExpr1 = null, boolExpr2 = null;
                    switch (bes.OperatorToken.ValueText){
                        case "==":
                            if (z3Left is FPExpr)
                                boolExpr = this.z3Context.MkFPEq((FPExpr)z3Left, (FPExpr)z3Right);
                        else
                            boolExpr = this.z3Context.MkEq(z3Left, z3Right);
                            this.solverAssertions.Enqueue(boolExpr);                        
                        break;
                        case "!=":
                        if (z3Left is FPExpr)
                            boolExpr = this.z3Context.MkFPEq((FPExpr)z3Left, (FPExpr)z3Right);
                        else
                            boolExpr = this.z3Context.MkEq(z3Left, z3Right);
                        boolExpr = this.z3Context.MkNot(boolExpr);
                            this.solverAssertions.Enqueue(boolExpr);
                        break;
                        case "<":
                        
                            if(z3Left is FPExpr)
                                boolExpr = this.z3Context.MkFPLt((FPExpr)z3Left, (FPExpr)z3Right);
                            else
                                boolExpr = this.z3Context.MkLt((ArithExpr)z3Left, (ArithExpr)z3Right);                            
                            this.solverAssertions.Enqueue(boolExpr);
                            break;
                    case "<=":
                            boolExpr1 = this.z3Context.MkEq(z3Left, z3Right);

                        if (z3Left is FPExpr)
                            boolExpr2 = this.z3Context.MkFPLt((FPExpr)z3Left, (FPExpr)z3Right);
                        else
                            boolExpr2 = this.z3Context.MkLt((ArithExpr)z3Left, (ArithExpr)z3Right);

                        boolExpr = this.z3Context.MkOr(boolExpr1, boolExpr2);
                        this.solverAssertions.Enqueue(boolExpr);
                            break;
                        case ">=":
                            boolExpr1 = this.z3Context.MkEq(z3Left, z3Right);
                        if (z3Left is FPExpr)
                            boolExpr2 = this.z3Context.MkFPGt((FPExpr)z3Left, (FPExpr)z3Right);
                        else
                            boolExpr2 = this.z3Context.MkGt((ArithExpr)z3Left, (ArithExpr)z3Right);

                        boolExpr = this.z3Context.MkOr(boolExpr1, boolExpr2);
                            this.solverAssertions.Enqueue(boolExpr);
                            break;
                        case ">":
                            if (z3Left is FPExpr)
                                boolExpr = this.z3Context.MkFPGt((FPExpr)z3Left, (FPExpr)z3Right);
                            else
                                boolExpr = this.z3Context.MkGt((ArithExpr)z3Left, (ArithExpr)z3Right);
                        this.solverAssertions.Enqueue(boolExpr);
                            break;
                       /*case "%":
                            boolExpr = this.z3Context.MkNot( this.z3Context.MkEq(this.z3Context.MkMod((IntExpr)z3Left, (IntExpr)z3Right), this.z3Context.MkBool(false)));
                            this.solverAssertions.Enqueue(boolExpr);
                            break;
                            */
                    default:
                            throw new Exception("Operator not found");
                }

                }
                else
                {
                    throw new Exception();
                }
                
            
        }

        private Expr GetExpr(string comparingTypeVarName, string valueText){
            TypeCode typeCode = Type.GetTypeCode(variableToType[comparingTypeVarName]);
            switch (typeCode){
                
                case TypeCode.Boolean:
                    return this.z3Context.MkBool(Convert.ToBoolean(valueText));
                    break;

                case TypeCode.Char:
                    return this.z3Context.MkInt(char.Parse(valueText.Replace("'", "")));
                case TypeCode.SByte:                
                case TypeCode.Byte:                    
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return this.z3Context.MkInt(valueText);

                case TypeCode.Single:                    
                case TypeCode.Double:                   
                case TypeCode.Decimal:
                    return this.z3Context.MkFP(GetVal(valueText), this.z3Context.MkFPSort128());
                
                case TypeCode.String:
                    return this.z3Context.MkString(valueText);

                case TypeCode.Empty:                    
                case TypeCode.Object:                    
                case TypeCode.DBNull:
                case TypeCode.DateTime:                    
                    MessageBox.Show("Complex types not supported");
                    throw new Exception("Complex types not supported");
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private double GetVal(string valueText){
            
            float fres;
            if (float.TryParse(valueText, out fres))
                return fres;
            double dres;
            if (double.TryParse(valueText, out dres))
                return dres;

            if (valueText.EndsWith("d"))
                if (double.TryParse(valueText.Substring(0, valueText.Length - 1), out dres))
                return dres;

            decimal decres;
            if (decimal.TryParse(valueText, out decres))
                return (double)decres;

            if (valueText.EndsWith("m"))
                
                    if (decimal.TryParse(valueText.Substring(0, valueText.Length - 1), out decres))
                return (double)decres;

            if (valueText.EndsWith("f"))
                if (float.TryParse(valueText.Substring(0, valueText.Length-1), out fres))
                    return fres;

            throw new Exception("Unable to determine floating point type");
        }

        private Dictionary<string, Expr> variableToZ3 = new Dictionary<string, Expr>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, Type> variableToType = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);
        private void ProcessVariableDeclarationSyntax(VariableDeclarationSyntax var){
            SyntaxToken identifier = var.Variables.FirstOrDefault().Identifier;
            string variableName = identifier.Text;
            
            Type t = GetType(var.Type.GetFirstToken().ValueText);
            variableToType[variableName] = t;
            TypeCode typeCode = Type.GetTypeCode(t);
            switch (typeCode){
                
                case TypeCode.Boolean:
                    variableToZ3[variableName] = this.z3Context.MkBoolConst(variableName);
                    break;
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:                    
                case TypeCode.UInt16:                    
                case TypeCode.Int32:                   
                case TypeCode.UInt32:                    
                case TypeCode.Int64:                    
                case TypeCode.UInt64:
                    variableToZ3[variableName] = this.z3Context.MkIntConst(variableName);
                    break;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    variableToZ3[variableName] = (FPExpr)this.z3Context.MkConst(variableName, this.z3Context.MkFPSort128());
                    break;
                
                case TypeCode.String:
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                    MessageBox.Show("Complex types not supported");
                    throw new Exception("Complex types not supported");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Type GetType(string typetext)
        {
            switch (typetext){
                case "int":
                    return typeof(int);
                case "bool":
                    return typeof(bool);
                case "float":
                    return typeof(float);
                case "char":
                    return typeof(char);
                default:
                    throw new Exception("No type");
            }
        }

        public void Solve1(){
            using (Context ctx = new Context()){
                Expr a = ctx.MkIntConst("a");
                
                IntExpr twelve = ctx.MkInt(12);

                Solver s = ctx.MkSolver();
                
                s.Assert(ctx.MkEq(a, twelve));
                Console.WriteLine(s.Check());

                Microsoft.Z3.Model m = s.Model;
                foreach (FuncDecl d in m.Decls)
                    Console.WriteLine(d.Name + " -> " + m.ConstInterp(d));

                Console.ReadLine();
            }
        }

    }
}
