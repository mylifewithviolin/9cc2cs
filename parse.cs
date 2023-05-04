using System; 
using System.Text.RegularExpressions;



namespace CC9
{
    public partial class Program
    {

        const string TOKEN_SYNBL_PLUS ="+";
        const string TOKEN_SYNBL_MINUS ="-";
        // �ȡ�����μ���
        enum TokenKind{
            TK_RESERVED, // ����
            TK_NUM,      // �����ȡ�����
            TK_EOF,      // ���Ϥν�����ɽ���ȡ�����
        };

        // ��ݹ�ʸ�ڤΥΡ��ɤμ���
        enum NodeKind{
            ND_ADD, // +
            ND_SUB, // -
            ND_MUL, // *
            ND_DIV, // /
            ND_EQ,  // ==
            ND_NE,  // !=
            ND_LT,  // <
            ND_LE,  // <=
            ND_NUM, // ����
        };

        class Token {
            public TokenKind kind; // �ȡ�����η�
            public  int next;    // �������ϥȡ�����
            public int val;        // kind��TK_NUM�ξ�硢���ο���
            public string? str;      // �ȡ�����ʸ����
            public int len;        // �ȡ������Ĺ��
            
        };
    
        // ��ݹ�ʸ�ڤΥΡ��ɤη�
        class  Node {
            public NodeKind kind; // �Ρ��ɤη�
            public Node? lhs;     // ����
            public Node? rhs;     // ����
            public int val;       // kind��ND_NUM�ξ��Τ߻Ȥ�
        };

        // ����ʸ����p��ȡ����ʥ������Ƥ�����֤�
        // ����ʸ�����ȡ����ʥ������ƥȡ�����ꥹ�Ȥ��֤�
        /// <summary>�ȡ����ʥ�������</summary>
        /// <param name="p">����ʸ����</param>
        /// <returns>�ȡ�����ꥹ��</returns>
        static List<Token> tokenize(string p) {
            int next = 1;
            Token cur = new();
            List<Token> tokenList =new List<Token>();

            var cs = p.ToCharArray();
            int i= 0;
            while(i<cs.Length) {
                // ����ʸ���򥹥��å�
                if (isspace(cs[i].ToString())) {
                    i++;
                    continue;
                }
                // Multi-letter punctuator
                if (iscomparisonhead(cs[i].ToString())) {
                    if(i+1<cs.Length){
                        var comp =cs[i].ToString()+cs[i+1].ToString();
                        if (iscomparison(comp)) {
                            cur = new_token(TokenKind.TK_RESERVED, next, comp,2);
                            tokenList.Add(cur);
                            next++;
                            i+=2;
                            continue;
                        }
                    }
                }

                // Single-letter punctuator
                if (isarithmetic(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                if (isdigit(cs[i].ToString())) {
        
                    string numberStr = cs[i].ToString();
                    i++;
                    if(i<cs.Length){
                        while(isdigit(cs[i].ToString())) {
                            numberStr+= cs[i].ToString();
                            i++;
                            if(i>=cs.Length)break;
                        }
                    }
                    cur = new_token(TokenKind.TK_NUM,next, numberStr,i);
                    cur.val = strtol(numberStr);
                    tokenList.Add(cur);
                    next++;
                    continue;
                }

                //�����˿ʤ�����㳰
                //Console.Write($"�ȡ����ʥ����Ǥ��ޤ���");
                i++;
            }

            cur = new_token(TokenKind.TK_EOF, next, "",0);
            tokenList.Add(cur);
            return tokenList;
        }


        /// <summary>��</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>����</returns>
        static Node expr(List<Token> tokenList,ref int curIndex) {
            return equality(tokenList,ref curIndex);
        }
        /// <summary>����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node equality(List<Token> tokenList,ref int curIndex) {
            Node node = relational(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"-=",ref curIndex)){
                    node = new_binary(NodeKind.ND_EQ, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"!=",ref curIndex)){
                    node = new_binary(NodeKind.ND_NE, node, relational(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else
                return node;
            }
        }

        /// <summary>�ط���</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node relational(List<Token> tokenList,ref int curIndex) {
            Node node = add(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"<",ref curIndex)){
                                    node = new_binary(NodeKind.ND_LT, node, primary(tokenList,ref curIndex));
                                token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"<=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,">",ref curIndex)){
                    node = new_binary(NodeKind.ND_LT, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,">=",ref curIndex)){
                    node = new_binary(NodeKind.ND_LE, add(tokenList,ref curIndex), node);
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else
                return node;
            }
        }
        /// <summary>�ø���</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node add(List<Token> tokenList,ref int curIndex) {
            Node node = mul(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"+",ref curIndex)){
                    //node = new_node(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_ADD, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else if (consume(token,"-",ref curIndex)){
                    //node = new_node(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_SUB, node, mul(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                    }
                else
                    return node;
            }
        }
        /// <summary>�����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node mul(List<Token> tokenList,ref int curIndex) {
            //Node node = primary(tokenList,ref curIndex);
            Node node = unary(tokenList,ref curIndex);
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            for (;;) {
                if (consume(token,"*",ref curIndex)){
                    //node = new_node(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_MUL, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else if (consume(token,"/",ref curIndex)){
                    //node = new_node(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    node = new_binary(NodeKind.ND_DIV, node, primary(tokenList,ref curIndex));
                    token = getToken(tokenList,curIndex);//���Υȡ�����
                }
                else
                return node;
            }
        }
        /// <summary>ñ�༰</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node unary(List<Token> tokenList,ref int curIndex) { 
                Token token = getToken(tokenList,curIndex);//���Υȡ�����
                if (consume(token,"+",ref curIndex)){
                    return unary(tokenList,ref curIndex);
                }
                else if (consume(token,"-",ref curIndex)){
                    return new_binary(NodeKind.ND_SUB,new_num(0), unary(tokenList,ref curIndex));
                }
                else
                return primary(tokenList,ref curIndex);
        }
        /// <summary>ͥ����</summary>
        /// <param name="tokenList">�ȡ�����ꥹ��</param>
        /// <param name="curIndex">������</param>
        /// <returns>�Ρ���</returns>
        static Node primary(List<Token> tokenList,ref int curIndex) {
            Token token = getToken(tokenList,curIndex);//���Υȡ�����
            // ���Υȡ�����"("�ʤ顢"(" expr ")"�ΤϤ�
            if (consume(token,"(",ref curIndex)) {
                Node node = expr(tokenList,ref curIndex);
                token = getToken(tokenList,curIndex);//���Υȡ�����
                expect(token,")",ref curIndex);
                token = getToken(tokenList,curIndex);//���Υȡ�����
                return node;
            }

            // �����Ǥʤ���п��ͤΤϤ�
            //return new_node_num(expect_number(token,ref curIndex));
            return new_num(expect_number(token,ref curIndex));
        }

            static Token getToken(List<Token> tokenList,int curIndex) {
                if(curIndex>=tokenList.Count) return new Token();
                Token token = tokenList.ElementAt(curIndex);//���Υȡ�����
                return token;
            }
            /// <summary>�������Ρ���</summary>
            /// <param name="kind">����</param>
            /// <returns>�Ρ���</returns>
            static Node new_node(NodeKind kind) {
                Node node = new();
                node.kind = kind;
                // node.lhs = lhs;
                // node.rhs = rhs;
                return node;
            }
            /// <summary>��������ʬ��</summary>
            /// <param name="kind">����</param>
            /// <param name="lhs">����</param>
            /// <param name="rhs">����</param>
            /// <returns>node</returns>
            static Node new_binary(NodeKind kind, Node lhs, Node rhs) {
                Node node = new_node(kind);
                node.lhs = lhs;
                node.rhs = rhs;
                return node;
            }


            static Node new_num(int val) {
                Node node = new_node(NodeKind.ND_NUM);
                node.val = val;
                return node;
            }


            // ���Υȡ����󤬴��Ԥ��Ƥ��뵭��ΤȤ��ˤϡ��ȡ������1���ɤ߿ʤ��
            // �����֤�������ʳ��ξ��ˤϵ����֤���
            /// <summary>���Ѥ���</summary>
            /// <param name="token">�ȡ�����</param>
            /// <param name="op">����ʸ��</param>
            /// <param name="next">������</param>
            /// <returns>bool</returns>
            static bool consume(Token token,string op, ref int next) {
                //if (token.kind != TokenKind.TK_RESERVED || token.str != op)
                if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len || 
                        token.str != op)
                    return false;
                next = token.next;
                return true;
            }
            // ���Υȡ����󤬴��Ԥ��Ƥ��뵭��ΤȤ��ˤϡ��ȡ������1���ɤ߿ʤ�롣
            // ����ʳ��ξ��ˤϥ��顼����𤹤롣
            /// <summary>ͽ�ۤ���</summary>
            /// <param name="token">�ȡ�����</param>
            /// <param name="op">����ʸ��</param>
            /// <param name="next">������</param>
            /// <returns>bool</returns>
        static  void expect(Token token,string op, ref int next) {
            if (token.kind != TokenKind.TK_RESERVED || op.Length  != token.len ||  token.str != op){
                       // Console.WriteLine($"{op}�ǤϤ���ޤ���");
                    }
            next = token.next;
        }

        // ���Υȡ����󤬿��ͤξ�硢�ȡ������1���ɤ߿ʤ�Ƥ��ο��ͤ��֤���
        // ����ʳ��ξ��ˤϥ��顼����𤹤롣
        static int expect_number(Token token, ref int next) {
            if (token.kind != TokenKind.TK_NUM){
                //Console.WriteLine($"���ǤϤ���ޤ���");
            }
            int val = token.val;
            next = token.next;
            return val;
        }

        // ���顼����𤹤뤿��δؿ�
        static void error(string fmt) {
            Console.WriteLine(fmt);
        }
        static bool at_eof(Token token) {
            return token.kind == TokenKind.TK_EOF;
        }

        // �������ȡ�������������cur�˷Ҥ���
            static Token new_token(TokenKind kind, int next, string str, int len) {
            Token tok = new();
            tok.kind = kind;
            tok.str = str;
            tok.len = len;
            tok.next = next;
            return tok;
        }
        static bool isspace( string input )
        {
            return( Regex.IsMatch( input,"\\s" ) );
        }
        static bool isarithmetic( string input )
        {
            return( Regex.IsMatch( input,"[\\+\\-\\*\\/\\(\\)\\<\\>]" ) );
        }
        static bool iscomparisonhead ( string input )
        {
            if(input =="="||input =="!"||input =="<"||input ==">"){
                return true;
            }else{
                return false;
            }       
        }

        static bool iscomparison ( string input )
        {
            if(input =="=="||input =="!="||input =="<="||input ==">="){
                return true;
            }else{
                return false;
            }       
        }

        static bool isdigit( string input )
        {
            return Regex.IsMatch( input, "[0-9]" );
        }
        static int strtol(string str){
            return  int.Parse(str);
        }

            
    }
}